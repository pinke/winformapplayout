﻿using log4net;
using Model.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess.Permission
{
    public class UserDAO : BaseDAO
    {
        private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string SP_Create = "procUsersInsert";
        private const string SP_Modify = "procUsersUpdate";
        private const string SP_Delete = "procUsersDelete";
        private const string SP_Select = "procUsersSelect";

        public UserDAO()
            : base()
        { 
        }

        public UserDAO(DbHelper dbHelper)
            : base(dbHelper)
        {
        }

        public int Create(User user)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Create);
            _dbHelper.AddInParameter(dbCommand, "@Operator", System.Data.DbType.String, user.Operator);
            _dbHelper.AddInParameter(dbCommand, "@Name", System.Data.DbType.String, user.Name);
            _dbHelper.AddInParameter(dbCommand, "@Status", System.Data.DbType.Int32, (int)user.Status);

            _dbHelper.AddReturnParameter(dbCommand, "@return", System.Data.DbType.Int32);

            int ret = _dbHelper.ExecuteNonQuery(dbCommand);

            int entrustId = -1;
            if (ret > 0)
            {
                entrustId = (int)dbCommand.Parameters["@return"].Value;
            }

            return entrustId;
        }

        public int Update(User user)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Modify);
            _dbHelper.AddInParameter(dbCommand, "@Operator", System.Data.DbType.String, user.Operator);
            _dbHelper.AddInParameter(dbCommand, "@Name", System.Data.DbType.String, user.Name);
            _dbHelper.AddInParameter(dbCommand, "@Status", System.Data.DbType.Int32, (int)user.Status);

            return _dbHelper.ExecuteNonQuery(dbCommand);
        }

        public int Delete(string operatorNo)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Delete);
            _dbHelper.AddInParameter(dbCommand, "@Operator", System.Data.DbType.String, operatorNo);
            
            return _dbHelper.ExecuteNonQuery(dbCommand);
        }

        public User Get(string operatorNo)
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Select);
            _dbHelper.AddInParameter(dbCommand, "@Operator", System.Data.DbType.String, operatorNo);

            User item = new User();
            var reader = _dbHelper.ExecuteReader(dbCommand);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    item.Id = (int)reader["Id"];
                    item.Operator = (string)reader["Operator"];
                    item.Name = (string)reader["Name"];
                    item.Status = (UserStatus)reader["Status"];
                    break;
                }
            }

            reader.Close();
            _dbHelper.Close(dbCommand.Connection);

            return item;
        }

        public List<User> Get()
        {
            var dbCommand = _dbHelper.GetStoredProcCommand(SP_Select);
            
            List<User> items = new List<User>();
            var reader = _dbHelper.ExecuteReader(dbCommand);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    User item = new User();
                    item.Id = (int)reader["Id"];
                    item.Operator = (string)reader["Operator"];
                    item.Name = (string)reader["Name"];
                    item.Status = (UserStatus)reader["Status"];

                    items.Add(item);
                }
            }

            reader.Close();
            _dbHelper.Close(dbCommand.Connection);

            return items;
        }
    }
}
