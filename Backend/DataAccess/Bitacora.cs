using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using Dominio.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace DataAccess
{
    public class Bitacora
    {
        private ModelContext _context;
        private IConfiguration _configuration;
        private IMemoryCache _memoryCache;
        private IHttpContextAccessor _httpContextAccessor;
        private HttpRequest _request;
        private SgOperacion _operacion;
        private List<RtTabla> _tablasBitacorizables; 
        private string _usuario;
        private string _usuarioBD;

        public Bitacora(ModelContext context, IConfiguration configuration, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _request = httpContextAccessor.HttpContext.Request;

            // Usuario de aplicación
            if (_request.Headers.TryGetValue("Usuario", out StringValues usuario))
                _usuario = usuario.First();
            else
                _usuario = "anonimo";

            // Usuario de base de datos
            DbConnectionStringBuilder connectionBuilder = new DbConnectionStringBuilder();

            connectionBuilder.ConnectionString = _context.Database.GetConnectionString();
            _usuarioBD = connectionBuilder["User Id"].ToString().ToUpper();
        }

        public void CargarTablasBitacorizables()
        {
            _tablasBitacorizables = _memoryCache.GetOrCreate(
                "tablas_bitacorizables",
                cacheEntry =>
                {
                    var contextOptions = new DbContextOptionsBuilder<ModelContext>()
                       .UseOracle(_configuration.GetConnectionString("MetaDataConnectionString"), x => x.UseOracleSQLCompatibility("11"))
                       .Options;

                    using (var context = new ModelContext(contextOptions, _configuration, _memoryCache, _httpContextAccessor))
                    {
                        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                        return context.RtTabla.Where(t => t.Bitacorizar == "S").Select(t => new RtTabla { Tabla = t.Tabla }).ToList();
                    }
                });
        }

        private bool EsBitacorizable(string nombreTabla)
        {
            CargarTablasBitacorizables();

            foreach (RtTabla tabla in _tablasBitacorizables)
            {
                if (tabla.Tabla.ToUpper() == nombreTabla.ToUpper())
                    return true;
            }

            return false;
        }

        public void CrearOperacion(DateTime fechaInicio)
        {
            // No crear si es consulta
            if (_request.Method == "GET")
                return;

            // No crear si no es accion estandar
            if (!_request.RouteValues.TryGetValue("controller", out object? objetoControlador) || !_request.RouteValues.TryGetValue("action", out object? objetoAccion))
                return;

            string nombreControlador = objetoControlador.ToString();
            string nombreAccion = objetoAccion.ToString();

            // No crear si es login
            if (nombreAccion == "Autenticacion")
                return;

            // No crear si es solicitud de reporte
            if (nombreAccion == "GetReportrAsync" || nombreAccion == "GetReportr")
                return;

            _operacion = new SgOperacion();

            // Obtener cuerpo de solicitud
            //var controlCuerpo = _request.HttpContext.Features.Get<IHttpBodyControlFeature>();
            //string cuerpo = string.Empty;

            //using (StreamReader lector = new StreamReader(_request.Body, Encoding.UTF8))
            //{
            //    controlCuerpo.AllowSynchronousIO = true;
            //    cuerpo = lector.ReadToEnd();
            //}

            _operacion.NombreOperacion = Assembly.GetEntryAssembly().GetName().Name + "." + nombreControlador + "." + nombreAccion;
            _operacion.Descripcion = string.Format("[{0}] {1}{2}", _request.Method, _request.Path, _request.QueryString);
            _operacion.Usuario = _usuario;
            _operacion.FechaOperacion = fechaInicio;
            _operacion.UsuarioBdd = _usuarioBD;
        }

        public void AgregarDetalle(EntityEntry entidad, DateTime fechaRegistro)
        {
            SgEncBitacora encabezado = new SgEncBitacora();

            // Codificar estado de entidad
            switch(entidad.State)
            {
                case EntityState.Added:
                    encabezado.Operacion = "I";
                    break;
                case EntityState.Modified:
                    encabezado.Operacion = "M";
                    break;
                case EntityState.Deleted:
                    encabezado.Operacion = "D";
                    break;
            }

            // No proceder si no se creó operacion
            if (_operacion == null)
                return;

            // No proceder si tabla no es bitacorizable
            if (!EsBitacorizable(entidad.Metadata.GetTableName()))
                return;

            // No proceder si no se controla tipo de operación
            if (string.IsNullOrEmpty(encabezado.Operacion))
                return;

            // Valores de encabezado
            encabezado.NombreTabla = entidad.Metadata.GetTableName();
            encabezado.Usuario = _usuario;
            encabezado.UsuarioBdd = _usuarioBD;
            encabezado.FechaRegistro = fechaRegistro;

            decimal indice = 0;

            foreach (PropertyEntry propiedad in entidad.Properties)
            {
                // Nombre de campo, valor original y nuevo
                string nombreColumna = propiedad.Metadata.GetColumnName();
                string valorOriginal = propiedad.OriginalValue != null ? propiedad.OriginalValue.ToString() : string.Empty;
                string nuevoValor = propiedad.CurrentValue != null ? propiedad.CurrentValue.ToString() : string.Empty;

                // Clave primaria de tabla en encabezado
                if (propiedad.Metadata.IsKey())
                {
                    if (string.IsNullOrEmpty(encabezado.CamposPk))
                    {
                        encabezado.CamposPk = nombreColumna;
                        encabezado.LlaveFila = nuevoValor;
                    }
                    else
                    {
                        encabezado.CamposPk += " , " + nombreColumna;
                        encabezado.LlaveFila += " , " + nuevoValor;
                    }
                }

                // Ingreso
                if (encabezado.Operacion == "I" && !string.IsNullOrEmpty(nuevoValor))
                {
                    SgDetalleBitacora detalle = new SgDetalleBitacora();

                    detalle.IdDetalle = indice;
                    detalle.NombreCampo = nombreColumna;
                    detalle.ValorNuevo = nuevoValor;

                    encabezado.SgDetalleBitacora.Add(detalle);
                }
                // Modificación
                else if (encabezado.Operacion == "M" && valorOriginal != nuevoValor)
                {
                    SgDetalleBitacora detalle = new SgDetalleBitacora();

                    detalle.IdDetalle = indice;
                    detalle.NombreCampo = nombreColumna;
                    detalle.ValorAnterior = valorOriginal;
                    detalle.ValorNuevo = nuevoValor;

                    encabezado.SgDetalleBitacora.Add(detalle);
                }
                // Eliminación
                else if (encabezado.Operacion == "D" && !string.IsNullOrEmpty(valorOriginal))
                {
                    SgDetalleBitacora detalle = new SgDetalleBitacora();

                    detalle.IdDetalle = indice;
                    detalle.NombreCampo = nombreColumna;
                    detalle.ValorAnterior = valorOriginal;

                    encabezado.SgDetalleBitacora.Add(detalle);
                }

                indice++;
            }

            // Asociar detalle a operación
            _operacion.SgEncBitacoras.Add(encabezado);
        }

        public void AsociarError(string descripcion, string stackTrace)
        {
            // No proceder si no se creó operación
            if (_operacion == null)
                return;
            
            SgError error = new SgError();

            error.Error = descripcion;
            error.Stacktrace = stackTrace;

            // Asociar error a operacion
            _operacion.SgErrores.Add(error);

            // Retirar detalles de operación
            _operacion.SgEncBitacoras = null;
        }

        public void Guardar(DateTime fechaFin)
        {
            // No proceder si no se creó operación
            if (_operacion == null)
                return;

            // Conexión independiente a base de datos especifica para almacenar bitacora
            var contextOptions = new DbContextOptionsBuilder<ModelContext>()
                       .UseOracle(_configuration.GetConnectionString("SigmConnectionString"), x => x.UseOracleSQLCompatibility("11"))
                       .Options;

            using (var context = new ModelContext(contextOptions, _configuration, _memoryCache, _httpContextAccessor))
            {
                // Deshabiitar bitacorizacion evitando bucle infinito
                context.Bitacorizar = false;

                // Almacenar bitacora de operación
                _operacion.FechaFinOperacion = fechaFin;
                context.SgOperaciones.Add(_operacion);

                context.SaveChanges();

                // Restablecer operación
                _operacion = null;
            }
        }
    }
}
