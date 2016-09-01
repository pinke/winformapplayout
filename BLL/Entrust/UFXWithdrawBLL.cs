﻿using BLL.UFX;
using BLL.UFX.impl;
using DBAccess;
using log4net;
using Model.Binding.BindingUtil;
using Model.BLL;
using Model.t2sdk;
using Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Entrust
{
    public class UFXWithdrawBLL
    {
        private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SecurityBLL _securityBLL = null;

        private EntrustDAO _entrustdao = new EntrustDAO();

        private int _timeOut = 30 * 1000;

        public UFXWithdrawBLL()
        {
            _securityBLL = BLLManager.Instance.SecurityBLL;
        }

        public BLLResponse Cancel(EntrustCommandItem cmdItem, List<EntrustSecurityItem> entrustItems, CallerCallback callerCallback)
        {
            BLLResponse bllResponse = new BLLResponse();

            List<UFXWithdrawRequest> requests = new List<UFXWithdrawRequest>();

            foreach (var entrustItem in entrustItems)
            {
                UFXWithdrawRequest request = new UFXWithdrawRequest
                {
                    EntrustNo = entrustItem.EntrustNo,
                };

                requests.Add(request);
            }

            Callbacker callbacker = new Callbacker
            {
                Token = new CallerToken
                {
                    SubmitId = cmdItem.SubmitId,
                    CommandId = cmdItem.CommandId,
                    WaitEvent = new AutoResetEvent(false),
                    Caller = callerCallback,
                },

                DataHandler = WithdrawDataHandler,
            };

            var result = _securityBLL.Withdraw(requests, callbacker);
            if (result == Model.ConnectionCode.Success)
            {
                if (callbacker.Token.WaitEvent.WaitOne(_timeOut))
                {
                    var errorResponse = callbacker.Token.OutArgs as UFXErrorResponse;
                    if (errorResponse != null && T2ErrorHandler.Success(errorResponse.ErrorCode))
                    {
                        bllResponse.Code = Model.ConnectionCode.Success;
                        bllResponse.Message = "Success Withdraw";
                    }
                    else
                    {
                        bllResponse.Code = Model.ConnectionCode.FailWithdraw;
                        bllResponse.Message = errorResponse.ErrorMessage;
                    }

                }
                else
                {
                    bllResponse.Code = Model.ConnectionCode.FailSubmit;
                    bllResponse.Message = "Fail to submit the basket withdraw to UFX!";
                }
            }
            else
            {
                bllResponse.Code = Model.ConnectionCode.FailSubmit;
                bllResponse.Message = "Fail to submit the basket withdraw to UFX!";
            }

            return bllResponse;
        }

        private int WithdrawDataHandler(CallerToken token, DataParser dataParser)
        {
            List<UFXBasketWithdrawResponse> responseItems = new List<UFXBasketWithdrawResponse>();

            var errorResponse = T2ErrorHandler.Handle(dataParser);
            token.OutArgs = errorResponse;

            if (dataParser.DataSets.Count > 1)
            {
                var dataFieldMap = UFXDataBindingHelper.GetProperty<UFXBasketWithdrawResponse>();
                for (int i = 1, count = dataParser.DataSets.Count; i < count; i++)
                {
                    var dataSet = dataParser.DataSets[i];
                    foreach (var dataRow in dataSet.Rows)
                    {
                        UFXBasketWithdrawResponse p = new UFXBasketWithdrawResponse();
                        UFXDataSetHelper.SetValue<UFXBasketWithdrawResponse>(ref p, dataRow.Columns, dataFieldMap);
                        responseItems.Add(p);
                    }
                }
            }

            int ret = -1;
            List<EntrustSecurityItem> entrustSecuItems = new List<EntrustSecurityItem>();
            if (token.SubmitId > 0)
            {
                foreach (var responseItem in responseItems)
                {
                    var entrustItem = new EntrustSecurityItem
                    {
                        SubmitId = token.SubmitId,
                        CommandId = token.CommandId,
                        SecuCode = responseItem.StockCode,
                        EntrustNo = responseItem.EntrustNo,
                    };

                    entrustSecuItems.Add(entrustItem);
                }

                if (entrustSecuItems.Count > 0)
                {
                    ret = _entrustdao.UpdateSecurityEntrustStatus(entrustSecuItems, Model.EnumType.EntrustStatus.CancelSuccess);
                    //ret = _entrustcmddao.UpdateEntrustCommandStatus(token.SubmitId, Model.EnumType.EntrustStatus.CancelSuccess);
                    //ret = _tradecmddao.UpdateTargetNumBySubmitId(token.SubmitId, token.CommandId);
                }
            }

            if (token.Caller != null)
            {
                token.Caller(token, entrustSecuItems, errorResponse);
            }

            if (token.WaitEvent != null)
            {
                token.WaitEvent.Set();
            }

            return ret;
        }
    }
}