﻿using log4net;
using Model.Permission;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess.Permission
{
    public class TokenResourcePermissionDAO : BaseDAO
    {
        private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string SP_Create = "procTokenResourcePermissionInsert";
        private const string SP_Modify = "procTokenResourcePermissionUpdate";
        private const string SP_Delete = "procTokenResourcePermissionDelete";
        private const string SP_SelectToken = "procTokenResourcePermissionSelectByToken";
        private const string SP_SelectResourceType = "procTokenResourcePermissionSelectResourceType";
        private const string SP_SelectByResource = "procTokenResourcePermissionSelectByResouce";
        private const string SP_SelectSingle = "procTokenResourcePermissionSelectSingle";

        public TokenResourcePermissionDAO()
            : base()
        { 
        }

        public TokenResourcePermissionDAO(DbHelper dbHelper)
            : base(dbHelper)
        {
        }

        public int Create(TokenResourcePermission permission)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Create);
            _dbHelper.AddInParameter(dbCommand, "@Token", System.Data.DbType.Int32, permission.Token);
            _dbHelper.AddInParameter(dbCommand, "@TokenType", System.Data.DbType.Int32, (int)permission.TokenType);
            _dbHelper.AddInParameter(dbCommand, "@ResourceId", System.Data.DbType.Int32, permission.ResourceId);
            _dbHelper.AddInParameter(dbCommand, "@ResourceType", System.Data.DbType.Int32, (int)permission.ResourceType);
            _dbHelper.AddInParameter(dbCommand, "@Permission", System.Data.DbType.Int32, permission.Permission);

            _dbHelper.AddReturnParameter(dbCommand, "@return", System.Data.DbType.Int32);

            int ret = _dbHelper.ExecuteNonQuery(dbCommand);

            int id = -1;
            if (ret > 0)
            {
                id = (int)dbCommand.Parameters["@return"].Value;
            }

            return id;
        }

        public int Update(TokenResourcePermission permission)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Modify);
            _dbHelper.AddInParameter(dbCommand, "@Token", System.Data.DbType.Int32, permission.Token);
            _dbHelper.AddInParameter(dbCommand, "@TokenType", System.Data.DbType.Int32, (int)permission.TokenType);
            _dbHelper.AddInParameter(dbCommand, "@ResourceId", System.Data.DbType.Int32, permission.ResourceId);
            _dbHelper.AddInParameter(dbCommand, "@ResourceType", System.Data.DbType.Int32, (int)permission.ResourceType);
            _dbHelper.AddInParameter(dbCommand, "@Permission", System.Data.DbType.Int32, permission.Permission);

            return _dbHelper.ExecuteNonQuery(dbCommand);
        }

        public int Delete(int resourceId, ResourceType resourceType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Delete);
            _dbHelper.AddInParameter(dbCommand, "@ResourceId", System.Data.DbType.Int32, resourceId);
            _dbHelper.AddInParameter(dbCommand, "@ResourceType", System.Data.DbType.Int32, (int)resourceType);

            return _dbHelper.ExecuteNonQuery(dbCommand);
        }

        public TokenResourcePermission GetSingle(int token, TokenType tokenType, int resourceId, ResourceType resourceType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_SelectSingle);
            _dbHelper.AddInParameter(dbCommand, "@Token", System.Data.DbType.Int32, token);
            _dbHelper.AddInParameter(dbCommand, "@TokenType", System.Data.DbType.Int32, (int)tokenType);
            _dbHelper.AddInParameter(dbCommand, "@ResourceId", System.Data.DbType.Int32, resourceId);
            _dbHelper.AddInParameter(dbCommand, "@ResourceType", System.Data.DbType.Int32, (int)resourceType);
            var reader = _dbHelper.ExecuteReader(dbCommand);

            TokenResourcePermission item = new TokenResourcePermission();
            if (reader.HasRows && reader.Read())
            {
                item = ParseData(reader);
            }

            reader.Close();
            _dbHelper.Close(dbCommand);

            return item;
        }

        public List<TokenResourcePermission> GetByToken(int token, TokenType tokenType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_SelectToken);
            _dbHelper.AddInParameter(dbCommand, "@Token", System.Data.DbType.Int32, token);
            _dbHelper.AddInParameter(dbCommand, "@TokenType", System.Data.DbType.Int32, (int)tokenType);

            List<TokenResourcePermission> items = new List<TokenResourcePermission>();
            var reader = _dbHelper.ExecuteReader(dbCommand);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    TokenResourcePermission item = ParseData(reader);

                    items.Add(item);
                }
            }

            reader.Close();
            _dbHelper.Close(dbCommand);

            return items;
        }

        public List<TokenResourcePermission> GetByResourceType(int token, TokenType tokenType, ResourceType resourceType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_SelectResourceType);
            _dbHelper.AddInParameter(dbCommand, "@Token", System.Data.DbType.Int32, token);
            _dbHelper.AddInParameter(dbCommand, "@TokenType", System.Data.DbType.Int32, (int)tokenType);
            _dbHelper.AddInParameter(dbCommand, "@ResourceType", System.Data.DbType.Int32, (int)resourceType);

            List<TokenResourcePermission> items = new List<TokenResourcePermission>();
            var reader = _dbHelper.ExecuteReader(dbCommand);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    TokenResourcePermission item = ParseData(reader);
                    
                    items.Add(item);
                }
            }

            reader.Close();
            _dbHelper.Close(dbCommand);

            return items;
        }

        public List<TokenResourcePermission> GetByResource(int resourceId, ResourceType resourceType)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_SelectByResource);
            _dbHelper.AddInParameter(dbCommand, "@ResourceId", System.Data.DbType.Int32, resourceId);
            _dbHelper.AddInParameter(dbCommand, "@ResourceType", System.Data.DbType.Int32, (int)resourceType);

            List<TokenResourcePermission> items = new List<TokenResourcePermission>();
            var reader = _dbHelper.ExecuteReader(dbCommand);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    TokenResourcePermission item = ParseData(reader);

                    items.Add(item);
                }
            }

            reader.Close();
            _dbHelper.Close(dbCommand);

            return items;
        }

        private TokenResourcePermission ParseData(DbDataReader reader)
        {
            TokenResourcePermission item = new TokenResourcePermission();
            item.Id = (int)reader["Id"];
            item.Token = (int)reader["Token"];
            item.TokenType = (TokenType)reader["TokenType"];
            item.ResourceId = (int)reader["ResourceId"];
            item.ResourceType = (ResourceType)reader["ResourceType"];
            item.Permission = (int)reader["Permission"];

            if (reader["CreateDate"] != null && reader["CreateDate"] != DBNull.Value)
            {
                item.CreateDate = (DateTime)reader["CreateDate"];
            }

            if (reader["ModifiedDate"] != null && reader["ModifiedDate"] != DBNull.Value)
            {
                item.ModifieDate = (DateTime)reader["ModifiedDate"];
            }

            return item;
        }
    }
}
