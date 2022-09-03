using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class SequenceValueGenerator: ValueGenerator<decimal>
    {
        private string _schema;
        private string _sequenceName;

        public SequenceValueGenerator(string schema, string sequenceName)
        {
            _schema = schema;
            _sequenceName = sequenceName;
        }

        public override bool GeneratesTemporaryValues => false;

        public override decimal Next(EntityEntry entry)
        {
            return NextValue(entry.Context);
        }
        public decimal NextValue(DbContext _context)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT {_schema}.{_sequenceName}.NEXTVAL FROM DUAL";
                _context.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    return reader.GetDecimal(0);
                }
            }
        }
    }
}
