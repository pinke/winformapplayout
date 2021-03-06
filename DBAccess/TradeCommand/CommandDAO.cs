﻿using log4net;
using Model.Database;
using Model.UI;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace DBAccess.TradeCommand
{
    public class CommandDAO: BaseDAO
    {
        private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string SP_CreateTradeCommand = "procTradeCommandInsert";
        private const string SP_ModifyTradeCommand = "procTradeCommandUpdate";
        private const string SP_CreateTradingSecurity = "procTradeCommandSecurityInsert";
        private const string SP_ModifyTradingSecurity = "procTradeCommandSecurityInsertOrUpdate";
        private const string SP_DeleteTradingSecurity = "procTradeCommandSecurityDelete";

        public CommandDAO()
            : base()
        { 
            
        }

        public CommandDAO(DbHelper dbHelper)
            : base(dbHelper)
        { 
            
        }

        /// <summary>
        /// Create the TradeCommand and TradeCommandSecurity by Transaction.
        /// </summary>
        /// <param name="cmdItem"></param>
        /// <param name="secuItems"></param>
        /// <returns>CommandId</returns>
        public int Create(Model.Database.TradeCommand cmdItem, List<TradeCommandSecurity> secuItems)
        {
            var dbCommand = _dbHelper.GetCommand();
            _dbHelper.Open(dbCommand);

            //use transaction to execute
            DbTransaction transaction = dbCommand.Connection.BeginTransaction();
            dbCommand.Transaction = transaction;
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            int ret = -1;
            int commandId = -1;
            try
            {
                dbCommand.CommandText = SP_CreateTradeCommand;

                _dbHelper.AddInParameter(dbCommand, "@InstanceId", System.Data.DbType.Int32, cmdItem.InstanceId);
                _dbHelper.AddInParameter(dbCommand, "@CommandNum", System.Data.DbType.Int32, cmdItem.CommandNum);
                _dbHelper.AddInParameter(dbCommand, "@CommandType", System.Data.DbType.Int32, cmdItem.ECommandType);
                _dbHelper.AddInParameter(dbCommand, "@ExecuteType", System.Data.DbType.Int32, cmdItem.EExecuteType);
                _dbHelper.AddInParameter(dbCommand, "@StockDirection", System.Data.DbType.Int32, (int)cmdItem.EStockDirection);
                _dbHelper.AddInParameter(dbCommand, "@FuturesDirection", System.Data.DbType.Int32, (int)cmdItem.EFuturesDirection);
                _dbHelper.AddInParameter(dbCommand, "@EntrustStatus", System.Data.DbType.Int32, (int)cmdItem.EEntrustStatus);
                _dbHelper.AddInParameter(dbCommand, "@DealStatus", System.Data.DbType.Int32, (int)cmdItem.EDealStatus);
                _dbHelper.AddInParameter(dbCommand, "@SubmitPerson", System.Data.DbType.Int32, cmdItem.SubmitPerson);
                
                //command time
                DateTime now = DateTime.Now;
                _dbHelper.AddInParameter(dbCommand, "@CreatedDate", System.Data.DbType.DateTime, now);
                _dbHelper.AddInParameter(dbCommand, "@StartDate", System.Data.DbType.DateTime, cmdItem.DStartDate);
                _dbHelper.AddInParameter(dbCommand, "@EndDate", System.Data.DbType.DateTime, cmdItem.DEndDate);
                _dbHelper.AddInParameter(dbCommand, "@Notes", System.Data.DbType.String, cmdItem.Notes);

                _dbHelper.AddReturnParameter(dbCommand, "@return", System.Data.DbType.Int32);

                ret = dbCommand.ExecuteNonQuery();

                if (ret >= 0)
                {
                    if (ret > 0)
                    {
                        commandId = (int)dbCommand.Parameters["@return"].Value;
                        cmdItem.CommandId = commandId;
                    }

                    foreach (var secuItem in secuItems)
                    {
                        dbCommand.Parameters.Clear();
                        dbCommand.CommandText = SP_CreateTradingSecurity;

                        secuItem.CommandId = commandId;
                        _dbHelper.AddInParameter(dbCommand, "@CommandId", System.Data.DbType.Int32, secuItem.CommandId);
                        _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, secuItem.SecuCode);
                        _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, (int)secuItem.SecuType);
                        //_dbHelper.AddInParameter(dbCommand, "@WeightAmount", System.Data.DbType.Int32, secuItem.WeightAmount);
                        _dbHelper.AddInParameter(dbCommand, "@CommandAmount", System.Data.DbType.Int32, secuItem.CommandAmount);
                        _dbHelper.AddInParameter(dbCommand, "@CommandDirection", System.Data.DbType.Int32, (int)secuItem.EDirection);
                        _dbHelper.AddInParameter(dbCommand, "@CommandPrice", System.Data.DbType.Double, secuItem.CommandPrice);

                        //string newid = string.Empty;
                        ret = dbCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                //TODO: add log
                logger.Error(ex);
                ret = -1;
                throw;
            }
            finally
            {
                _dbHelper.Close(dbCommand);
                transaction.Dispose();
            }

            return commandId;
        }

        public int Update(Model.Database.TradeCommand cmdItem, List<TradeCommandSecurity> modifiedSecuItems, List<TradeCommandSecurity> cancelSecuItems)
        {
            var dbCommand = _dbHelper.GetCommand();
            _dbHelper.Open(dbCommand);

            //use transaction to execute
            DbTransaction transaction = dbCommand.Connection.BeginTransaction();
            dbCommand.Transaction = transaction;
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            int ret = -1;
            int commandId = cmdItem.CommandId;
            try
            {
                dbCommand.CommandText = SP_ModifyTradeCommand;

                _dbHelper.AddInParameter(dbCommand, "@CommandId", System.Data.DbType.Int32, commandId);
                _dbHelper.AddInParameter(dbCommand, "@CommandStatus", System.Data.DbType.Int32, (int)cmdItem.ECommandStatus);
                _dbHelper.AddInParameter(dbCommand, "@ModifiedDate", System.Data.DbType.DateTime, cmdItem.ModifiedDate);
                _dbHelper.AddInParameter(dbCommand, "@StartDate", System.Data.DbType.DateTime, cmdItem.DStartDate);
                _dbHelper.AddInParameter(dbCommand, "@EndDate", System.Data.DbType.DateTime, cmdItem.DEndDate);
                string notes = (cmdItem.Notes != null) ? cmdItem.Notes : string.Empty;
                _dbHelper.AddInParameter(dbCommand, "@Notes", System.Data.DbType.String, notes);
                string modifiedCause = (cmdItem.ModifiedCause != null) ? cmdItem.ModifiedCause : string.Empty;
                _dbHelper.AddInParameter(dbCommand, "@ModifiedCause", System.Data.DbType.String, modifiedCause);
                string cancelCause = (cmdItem.CancelCause != null) ? cmdItem.CancelCause : string.Empty;
                _dbHelper.AddInParameter(dbCommand, "@CancelCause", System.Data.DbType.String, cancelCause);

                ret = dbCommand.ExecuteNonQuery();

                if (ret >= 0)
                {
                    foreach (var secuItem in modifiedSecuItems)
                    {
                        dbCommand.Parameters.Clear();
                        dbCommand.CommandText = SP_ModifyTradingSecurity;

                        secuItem.CommandId = commandId;
                        _dbHelper.AddInParameter(dbCommand, "@CommandId", System.Data.DbType.Int32, commandId);
                        _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, secuItem.SecuCode);
                        _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, (int)secuItem.SecuType);
                        //_dbHelper.AddInParameter(dbCommand, "@WeightAmount", System.Data.DbType.Int32, secuItem.WeightAmount);
                        _dbHelper.AddInParameter(dbCommand, "@CommandAmount", System.Data.DbType.Int32, secuItem.CommandAmount);
                        _dbHelper.AddInParameter(dbCommand, "@CommandDirection", System.Data.DbType.Int32, (int)secuItem.EDirection);
                        _dbHelper.AddInParameter(dbCommand, "@CommandPrice", System.Data.DbType.Double, secuItem.CommandPrice);

                        //string newid = string.Empty;
                        ret = dbCommand.ExecuteNonQuery();
                    }

                    foreach (var secuItem in cancelSecuItems)
                    {
                        dbCommand.Parameters.Clear();
                        dbCommand.CommandText = SP_DeleteTradingSecurity;

                        secuItem.CommandId = commandId;
                        _dbHelper.AddInParameter(dbCommand, "@CommandId", System.Data.DbType.Int32, commandId);
                        _dbHelper.AddInParameter(dbCommand, "@SecuCode", System.Data.DbType.String, secuItem.SecuCode);
                        _dbHelper.AddInParameter(dbCommand, "@SecuType", System.Data.DbType.Int32, (int)secuItem.SecuType);

                        //string newid = string.Empty;
                        ret = dbCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                //TODO: add log
                logger.Error(ex);
                ret = -1;
                throw;
            }
            finally
            {
                _dbHelper.Close(dbCommand);
                transaction.Dispose();
            }

            return commandId;
        }
    }
}
