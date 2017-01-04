﻿using BLL.EntrustCommand;
using BLL.UFX;
using BLL.UFX.impl;
using Config.ParamConverter;
using log4net;
using Model.Binding.BindingUtil;
using Model.UFX;
using System.Collections.Generic;

namespace BLL.Entrust.subscriber
{
    public class UFXWithdrawFailedBLL : IUFXSubsriberBLLBase
    {
        private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private EntrustSecurityBLL _entrustSecurityBLL = new EntrustSecurityBLL();

        public UFXWithdrawFailedBLL()
        { 
        
        }

        public int Handle(DataParser dataParser)
        {
            List<UFXWithdrawFailedResponse> responseItems = new List<UFXWithdrawFailedResponse>();
            var dataFieldMap = UFXDataBindingHelper.GetProperty<UFXWithdrawFailedResponse>();

            var errorResponse = T2ErrorHandler.Handle(dataParser);
            if (T2ErrorHandler.Success(errorResponse.ErrorCode))
            {
                //TODO:
                for (int i = 0, count = dataParser.DataSets.Count; i < count; i++)
                {
                    var dataSet = dataParser.DataSets[i];
                    foreach (var dataRow in dataSet.Rows)
                    {
                        UFXWithdrawFailedResponse p = new UFXWithdrawFailedResponse();
                        UFXDataSetHelper.SetValue<UFXWithdrawFailedResponse>(ref p, dataRow.Columns, dataFieldMap);
                        responseItems.Add(p);
                    }
                }
            }

            //update the database
            if (responseItems.Count > 0)
            {
                foreach (var responseItem in responseItems)
                {
                    int commandId;
                    int submitId;
                    int requestId;

                    //TODO: add log
                    if (EntrustRequestHelper.ParseThirdReff(responseItem.ThirdReff, out commandId, out submitId, out requestId))
                    {
                        _entrustSecurityBLL.UpdateEntrustStatus(submitId, commandId, responseItem.StockCode, Model.EnumType.EntrustStatus.CancelFail);
                    }
                    else
                    {
                        string msg = string.Format("Fail to parse the third_reff: [{0}], entrust_no: [{1}].", responseItem.ThirdReff, responseItem.EntrustNo);
                        logger.Error(msg);
                    }
                }
            }

            return responseItems.Count;
        }

    }
}
