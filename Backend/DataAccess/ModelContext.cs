using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Dominio.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace DataAccess
{
    public partial class ModelContext : DbContext
    {        
        public ModelContext(DbContextOptions<ModelContext> options, IConfiguration configuration, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                _bitacorizar = true;
                _bitacora = new Bitacora(this, configuration, memoryCache, httpContextAccessor);
                _bitacora.CrearOperacion(DateTime.Now);
            }
        }

        public virtual DbSet<RtTabla> RtTabla { get; set; }
        public virtual DbSet<SgDetalleBitacora> SgDetalleBitacora { get; set; }
        public virtual DbSet<SgEncBitacora> SgEncBitacora { get; set; }
        public virtual DbSet<SgError> SgErrores { get; set; }
        public virtual DbSet<SgOperacion> SgOperaciones { get; set; }

        private bool _bitacorizar;
        private Bitacora _bitacora;

        public bool Bitacorizar { get => _bitacorizar; set => _bitacorizar = value; }
        public Bitacora Bitacora { get => _bitacora; }

        private void ModelContext_SavingChanges(object sender, SavingChangesEventArgs e)
        {
            if (_bitacorizar)
            {
                // Agregar detalle de bitacora
                foreach (EntityEntry entidad in this.ChangeTracker.Entries())
                {
                    _bitacora.AgregarDetalle(entidad, DateTime.Today);
                }
            }
        }

        public List<T> ExecuteSqlRaw<T>(string query, Func<DbDataReader, T> map)
        {
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                this.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.Write);
            optionsBuilder.EnableSensitiveDataLogging();

            SavingChanges += ModelContext_SavingChanges;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("USR_PROYECTO");

            modelBuilder.Entity<RtTabla>(entity =>
            {
                entity.HasKey(e => e.Tid)
                    .HasName("TAB_PK");

                entity.ToTable("RT_TABLAS", "USR_PROYECTO");

                entity.Property(e => e.Tid)
                    .HasPrecision(18)
                    .ValueGeneratedNever()
                    .HasColumnName("TID");

                entity.Property(e => e.Bitacorizar)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("BITACORIZAR");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");

                entity.Property(e => e.MeClase)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("ME_CLASE");

                entity.Property(e => e.MeEstado)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("ME_ESTADO");

                entity.Property(e => e.MeOperacion)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("ME_OPERACION");

                entity.Property(e => e.Reportes)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("REPORTES")
                    .IsFixedLength(true);

                entity.Property(e => e.Sinonimo)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("SINONIMO");

                entity.Property(e => e.Sistema)
                    .HasPrecision(18)
                    .HasColumnName("SISTEMA");

                entity.Property(e => e.Tabla)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("TABLA");
            });

            modelBuilder.Entity<SgDetalleBitacora>(entity =>
            {
                entity.HasKey(e => new { e.IdDetalle, e.IdEnc, e.NombreCampo })
                    .HasName("DET_BIT_PK");

                entity.ToTable("SG_DETALLE_BITACORA", "USR_PROYECTO");

                entity.Property(e => e.IdDetalle)
                    .HasPrecision(10)
                    .HasColumnName("ID_DETALLE");

                entity.Property(e => e.IdEnc)
                    .HasPrecision(10)
                    .HasColumnName("ID_ENC");

                entity.Property(e => e.NombreCampo)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_CAMPO");

                entity.Property(e => e.ValorAnterior)
                    .HasMaxLength(2048)
                    .IsUnicode(false)
                    .HasColumnName("VALOR_ANTERIOR");

                entity.Property(e => e.ValorNuevo)
                    .HasMaxLength(2048)
                    .IsUnicode(false)
                    .HasColumnName("VALOR_NUEVO");

                entity.HasOne(d => d.IdEncNavigation)
                    .WithMany(p => p.SgDetalleBitacora)
                    .HasForeignKey(d => d.IdEnc)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ENC_DET_FK");
            });

            modelBuilder.Entity<SgEncBitacora>(entity =>
            {
                entity.HasKey(e => e.IdEnc)
                    .HasName("ENC_PK");

                entity.ToTable("SG_ENCBITACORA", "USR_PROYECTO");

                entity.HasIndex(e => e.IdOperacion, "ID_OPERACION_IDX");

                entity.Property(e => e.IdEnc)
                    .HasPrecision(10)
                    .HasColumnName("ID_ENC")
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator((_, __) => new SequenceValueGenerator("BITACORA", "SQ_ENCBITACORA"));

                entity.Property(e => e.CamposPk)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("CAMPOS_PK");

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("DATE")
                    .HasColumnName("FECHA_REGISTRO");

                entity.Property(e => e.IdOperacion)
                    .HasPrecision(10)
                    .HasColumnName("ID_OPERACION");

                entity.Property(e => e.LlaveFila)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("LLAVE_FILA");

                entity.Property(e => e.NombreTabla)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_TABLA");

                entity.Property(e => e.Operacion)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("OPERACION");

                entity.Property(e => e.Usuario)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("USUARIO");

                entity.Property(e => e.UsuarioBdd)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("USUARIO_BDD");

                entity.HasOne(d => d.IdOperacionNavigation)
                    .WithMany(p => p.SgEncBitacoras)
                    .HasForeignKey(d => d.IdOperacion)
                    .HasConstraintName("OPE_ENC_FK");
            });

            modelBuilder.Entity<SgError>(entity =>
            {
                entity.HasKey(e => e.IdOperacion)
                    .HasName("ERRORES_PK");

                entity.ToTable("SG_ERRORES", "USR_PROYECTO");

                entity.Property(e => e.IdOperacion)
                    .HasPrecision(10)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_OPERACION");

                entity.Property(e => e.Error)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("ERROR");

                entity.Property(e => e.Stacktrace)
                    .HasMaxLength(3900)
                    .IsUnicode(false)
                    .HasColumnName("STACKTRACE");

                entity.HasOne(d => d.IdOperacionNavigation)
                   .WithMany(p => p.SgErrores)
                   .HasForeignKey(d => d.IdOperacion);
            });

            modelBuilder.Entity<SgOperacion>(entity =>
            {
                entity.HasKey(e => e.IdOperacion)
                    .HasName("OPE_PK");

                entity.ToTable("SG_OPERACIONES", "USR_PROYECTO");

                entity.HasIndex(e => new { e.NombreOperacion, e.Usuario, e.FechaFinOperacion }, "IDX_OPERACIONES_USUARIO");

                entity.Property(e => e.IdOperacion)
                    .HasPrecision(10)
                    .HasColumnName("ID_OPERACION")
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator((_, __) => new SequenceValueGenerator("BITACORA", "SQ_OPERACION"));

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");

                entity.Property(e => e.FechaFinOperacion)
                    .HasColumnType("DATE")
                    .HasColumnName("FECHA_FIN_OPERACION");

                entity.Property(e => e.FechaOperacion)
                    .HasColumnType("DATE")
                    .HasColumnName("FECHA_OPERACION");

                entity.Property(e => e.Finalizada)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("FINALIZADA")
                    .HasDefaultValueSql("'S'");

                entity.Property(e => e.NombreOperacion)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_OPERACION");

                entity.Property(e => e.TiempoOperacionBdd)
                    .HasPrecision(18)
                    .HasColumnName("TIEMPO_OPERACION_BDD");

                entity.Property(e => e.Usuario)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("USUARIO");

                entity.Property(e => e.UsuarioBdd)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("USUARIO_BDD");
            });

          

            OnModelCreatingPartial(modelBuilder);
        }
    }
}
