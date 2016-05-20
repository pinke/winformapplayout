﻿using Model.SecurityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess
{
    public class SecurityInfoDAO: BaseDAO
    {
        private const string SP_Create = "procSecurityInfoInsert";
        private const string SP_Modify = "procSecurityInfoUpdate";
        private const string SP_Delete = "procSecurityInfoDelete";
        private const string SP_Get = "procSecurityInfoSelect";

        public SecurityInfoDAO()
            : base()
        { 
            
        }

        public SecurityInfoDAO(DbHelper dbHelper)
            : base(dbHelper)
        { 
            
        }

        public int Create(SecurityItem securityItem)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Create);
            _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, securityItem.SecuCode);
            _dbHelper.AddInParameter(dbCommand, "@SecuName", System.Data.DbType.String, securityItem.SecuName);
            _dbHelper.AddInParameter(dbCommand, "@ExchangeCode", System.Data.DbType.String, securityItem.ExchangeCode);
            _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, securityItem.SecuType);
            _dbHelper.AddInParameter(dbCommand, "@ListDate", System.Data.DbType.Int32, securityItem.ListDate);

            int ret = _dbHelper.ExecuteNonQuery(dbCommand);
            
            return ret;
        }

        public int Update(SecurityItem securityItem)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Modify);
            _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, securityItem.SecuCode);
            _dbHelper.AddInParameter(dbCommand, "@SecuName", System.Data.DbType.String, securityItem.SecuName);
            _dbHelper.AddInParameter(dbCommand, "@ExchangeCode", System.Data.DbType.String, securityItem.ExchangeCode);
            _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, securityItem.SecuType);

            int ret = _dbHelper.ExecuteNonQuery(dbCommand);
            return ret;
        }

        public int Delete(string secuCode, int secuType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Delete);
            _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, secuCode);

            if (secuType > 0)
            {
                _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, secuType);
            }
            
            int ret = _dbHelper.ExecuteNonQuery(dbCommand);
            
            return ret;
        }

        /// <summary>
        /// Get all the specified secuType if the secuType is -1. Get all types of securities.
        /// </summary>
        /// <param name="secuType"></param>
        /// <returns></returns>
        public List<SecurityItem> Get(int secuType)
        {
            return Select("", secuType);
        }

        /// <summary>
        /// Get the specific type of the security
        /// </summary>
        /// <param name="SecuCode"></param>
        /// <param name="secuType"></param>
        /// <returns></returns>
        public List<SecurityItem> Get(string SecuCode, int secuType)
        {
            return Select(SecuCode, secuType);
        }

        private List<SecurityItem> Select(string secuCode, int secuType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Get);
            if (!string.IsNullOrEmpty(secuCode))
            {
                _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, secuCode);
            }

            if (secuType > 0)
            {
                _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, secuType);
            }
            else if (secuType == -1)
            { 
                _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32,  null);
            }

            List<SecurityItem> items = new List<SecurityItem>();
            var reader = _dbHelper.ExecuteReader(dbCommand);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    SecurityItem item = new SecurityItem();
                    item.SecuCode = (string)reader["SecuCode"];
                    item.SecuName = (string)reader["SecuName"];
                    item.ExchangeCode = (string)reader["ExchangeCode"];
                    item.SecuType = (int)reader["SecuType"];
                    if (reader["ListDate"] != null && reader["ListDate"] != DBNull.Value)
                    {
                        item.ListDate = (string)reader["ListDate"];
                    }

                    items.Add(item);
                }
            }
            reader.Close();
            _dbHelper.Close(dbCommand.Connection);

            return items;
        }
    }
}
