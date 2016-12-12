﻿using BLL.Entrust;
using BLL.UFX.impl;
using Config;
using DBAccess.EntrustCommand;
using Model;
using Model.BLL;
using Model.Database;
using Model.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BLL.Frontend
{
    public class EntrustBLL
    {
        private EntrustDAO _entrustdao = new EntrustDAO();
        
        private UFXBasketEntrustBLL _ufxBasketEntrustBLL = new UFXBasketEntrustBLL();
        
        public EntrustBLL()
        { 
        }

        #region create

        public BLLResponse SubmitOne(Model.Database.EntrustCommand cmdItem, List<CancelRedoItem> cancelItems, CallerCallback callback)
        {
            //TODO: adjust the EntrustAmount
            List<EntrustSecurity> entrustItems = new List<EntrustSecurity>();
            DateTime now = DateTime.Now;

            //merge the same security in with the same commandId
            var uniqueSecuCodes = cancelItems.Select(p => p.SecuCode).Distinct().ToList();
            foreach (var secuCode in uniqueSecuCodes)
            {
                EntrustSecurity item = new EntrustSecurity
                {
                    CommandId = cmdItem.CommandId,
                    SecuCode = secuCode,
                    EntrustDate = now,
                };

                var originSecuItems = cancelItems.Where(p => p.SecuCode.Equals(secuCode)).ToList();
                if (originSecuItems != null && originSecuItems.Count > 0)
                {
                    item.SecuType = originSecuItems[0].SecuType;
                    item.EntrustPrice = originSecuItems[0].EntrustPrice;
                    item.EntrustDirection = originSecuItems[0].EDirection;
                    item.EntrustPriceType = originSecuItems[0].EEntrustPriceType;
                    item.PriceType = originSecuItems[0].EPriceSetting;

                    item.EntrustAmount = originSecuItems.Sum(o => o.LeftAmount);

                    entrustItems.Add(item);
                }
            }

            return SubmitOne(cmdItem, entrustItems, callback);
        }

        public BLLResponse SubmitOne(Model.Database.EntrustCommand cmdItem, List<EntrustSecurity> entrustItems, CallerCallback callback)
        {
            int ret = SumbitToDB(cmdItem, entrustItems);
            if (ret <= 0)
            {
                return new BLLResponse(ConnectionCode.DBInsertFail, "Fail to submit into database");
            }

            entrustItems.Where(p => p.CommandId == cmdItem.CommandId)
                .ToList()
                .ForEach(o => o.SubmitId = cmdItem.SubmitId);

            return _ufxBasketEntrustBLL.Submit(cmdItem, entrustItems, callback);
        }

        #endregion

        #region private

        private int SumbitToDB(Model.Database.EntrustCommand cmdItem, List<EntrustSecurity> entrustItems)
        {
            int userId = LoginManager.Instance.GetUserId();
            cmdItem.SubmitPerson = userId;

            return _entrustdao.Submit(cmdItem, entrustItems);
        }

        private Model.Database.EntrustCommand MergeEntrustCommandItem(List<Model.Database.EntrustCommand> oldCmdItems)
        {
            Debug.Assert(oldCmdItems.Select(p => p.CommandId).Distinct().Count() == 1, "撤销的委托指令不是同一指令号");
            var commandId = oldCmdItems.Select(p => p.CommandId).Distinct().Single();
            var copies = oldCmdItems.Where(p => p.CommandId == commandId).Select(o => o.Copies).Sum();
            var cmdItem = new Model.Database.EntrustCommand
            {
                CommandId = commandId,
                Copies = copies,
            };

            return cmdItem;
        }

        #endregion
    }
}
