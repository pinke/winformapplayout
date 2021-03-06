﻿using Config;
using hundsun.t2sdk;
using log4net;
using Model;
using Model.Binding.BindingUtil;
using Model.config;
using Model.UFX;
using System;
using System.Collections.Generic;
using System.Text;

namespace UFX.impl
{
    public class UFXBLLBase
    {
        private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected T2SDKWrap _t2SDKWrap;
        protected DataHandlerCallback _dataHandler;
        protected Dictionary<UFXFunctionCode, Dictionary<int, Callbacker>> _dataHandlerMap = new Dictionary<UFXFunctionCode, Dictionary<int, Callbacker>>();

        public UFXBLLBase(T2SDKWrap t2SDKWrap)
        {
            _t2SDKWrap = t2SDKWrap;
            _dataHandler = HandlData;
        }

        private int HandlData(UFXFunctionCode functionCode, int hSend, DataParser parser)
        {
            //FunctionCode functionCode = (FunctionCode)parser.FunctionCode;
            if (_dataHandlerMap.ContainsKey(functionCode))
            {
                var callbacker = GetDataHandler(functionCode, hSend);
                if (callbacker != null)
                {
                    var token = callbacker.Token;
                    var callback = callbacker.DataHandler;
                    if (callback != null)
                    {
                        callback(token, parser);
                    }

                    return 1;
                }
                else
                {
                    string msg = string.Format("提交UFX请求时，功能号[{0}]的回调方法，未注册的句柄ID[{0}]！", functionCode, hSend);
                    logger.Error(msg);

                    return -1;
                }
            }
            else
            {
                string msg = string.Format("提交UFX请求时，未注册功能号[{0}]的回调方法！", functionCode);
                logger.Error(msg);

                return -1;
            }
        }

        #region protect method

        /// <summary>
        /// 向UFX封装的接口T2SDKWrap注册回调，对所有的功能号注册相同的回调，回调函数触发之后，再在其中处理找到本次调用实际需要处理的
        /// 后续方法。
        /// 这里的回调顺序是：
        /// 1.T2SDKWrap中回调用_dataHandler
        /// 2._dataHandler中找到本次实际调用者注册的回调函数，调用者信息在_dataHandlerMap保存
        /// 3.如果存在调用者注册的回调函数，则调用。
        /// </summary>
        /// <param name="functionCode"></param>
        protected void RegisterUFX(UFXFunctionCode functionCode)
        {
            _t2SDKWrap.Register(functionCode, _dataHandler);
        }

        /// <summary>
        /// 该函数不可被调用，目前不可以删除
        /// </summary>
        /// <param name="functionCode"></param>
        protected void UnRegisterUFX(UFXFunctionCode functionCode)
        {
            _t2SDKWrap.UnRegister(functionCode);
        }

        protected void AddDataHandler(UFXFunctionCode functionCode, int hSend, Callbacker callbacker)
        {
            if (_dataHandlerMap.ContainsKey(functionCode))
            {
                if (_dataHandlerMap[functionCode].ContainsKey(hSend))
                {
                    _dataHandlerMap[functionCode][hSend] = callbacker;
                }
                else
                {
                    _dataHandlerMap[functionCode].Add(hSend, callbacker);
                }
            }
            else
            {
                _dataHandlerMap[functionCode] = new Dictionary<int, Callbacker>();
                _dataHandlerMap[functionCode].Add(hSend, callbacker);
            }
        }

        //protected void RemoveDataHandler(FunctionCode funtionCode, Callbacker callbacker)
        //{
        //    if (_dataHandlerMap.ContainsKey(funtionCode))
        //    {
        //        _dataHandlerMap[funtionCode].Dequeue(callbacker);
        //    }
        //}

        protected Callbacker GetDataHandler(UFXFunctionCode functionCode, int hSend)
        {
            Callbacker callbacker = null;

            if (_dataHandlerMap.ContainsKey(functionCode))
            {
                var sendCbMap = _dataHandlerMap[functionCode];
                if (sendCbMap.ContainsKey(hSend))
                {
                    callbacker = sendCbMap[hSend];
                    sendCbMap.Remove(hSend);
                }
            }

            return callbacker;
        }

        #endregion

        #region send request
        /// <summary>
        /// 异步调用UFX接口，完成调用之后，如果不出错，则注册回调信息
        /// </summary>
        /// <typeparam name="T">A generic type defines the UFX request parameters.</typeparam>
        /// <param name="functionCode">An enum type defines the UFX interface number.</param>
        /// <param name="requests">A generic request list. NOTE: the caller MUST control the request count if the
        /// interface does not support many requests at a time.
        /// </param>
        /// <param name="callbacker">It is used to store the callback information.</param>
        /// <returns>It is used to define the call result.</returns>
        public ConnectionCode SubmitAsync<T>(UFXFunctionCode functionCode, List<T> requests, Callbacker callbacker)
        {
            FunctionItem functionItem = ConfigManager.Instance.GetFunctionConfig().GetFunctionItem(functionCode);
            if (functionItem == null || functionItem.RequestFields == null || functionItem.RequestFields.Count == 0)
            {
                string msg = string.Format("提交UFX请求号[{0}]未定义！", functionCode);
                logger.Error(msg);
                return ConnectionCode.ErrorNoFunctionCode;
            }

            string userToken = LoginManager.Instance.LoginUser.Token;
            if (string.IsNullOrEmpty(userToken))
            {
                string msg = string.Format("提交UFX请求[{0}]令牌失效！", functionCode);
                logger.Error(msg);
                return ConnectionCode.ErrorLogin;
            }

            CT2BizMessage bizMessage = new CT2BizMessage();
            //初始化
            bizMessage.SetFunction((int)functionCode);
            bizMessage.SetPacketType(CT2tag_def.REQUEST_PACKET);

            //业务包
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();

            foreach (FieldItem item in functionItem.RequestFields)
            {
                packer.AddField(item.Name, item.Type, item.Width, item.Scale);
            }

            var dataFieldMap = UFXDataBindingHelper.GetProperty<T>();
            foreach (var request in requests)
            {
                foreach (FieldItem item in functionItem.RequestFields)
                {
                    if (dataFieldMap.ContainsKey(item.Name))
                    {
                        SetRequestField<T>(ref packer, request, item, dataFieldMap);
                    }
                    else
                    {
                        SetRequestDefaultField(ref packer, item, userToken);
                    }
                }
            }
            packer.EndPack();

#if DEBUG
            OutputParam<T>(functionCode, requests);
#endif
            unsafe
            {
                bizMessage.SetContent(packer.GetPackBuf(), packer.GetPackLen());
            }

            ConnectionCode retCode = ConnectionCode.Success;
            int hSend = _t2SDKWrap.SendAsync(bizMessage);
            if (hSend < 0)
            {
                string msg = string.Format("提交UFX请求[{0}]失败, 返回值：[{1}]！", functionCode, hSend);
                logger.Error(msg);
                retCode = ConnectionCode.ErrorConn;
            }
            else
            {
                //注册UFX返回数据后，需要调用的回调
                //此处存在假设，异步提交返回之前，不会触发回调
                AddDataHandler(functionCode, hSend, callbacker);
                retCode = ConnectionCode.Success;
            }

            packer.Dispose();
            bizMessage.Dispose();
            
            return retCode;
        }

        public DataParser SubmitSync<T>(UFXFunctionCode functionCode, List<T> requests)
        {
            DataParser parser = new DataParser();

            FunctionItem functionItem = ConfigManager.Instance.GetFunctionConfig().GetFunctionItem(functionCode);
            if (functionItem == null || functionItem.RequestFields == null || functionItem.RequestFields.Count == 0)
            {
                string msg = string.Format("提交UFX请求号[{0}]未定义！", functionCode);
                logger.Error(msg);
                parser.ErrorCode = ConnectionCode.ErrorNoFunctionCode;

                return parser;
            }

            string userToken = LoginManager.Instance.LoginUser.Token;
            if (string.IsNullOrEmpty(userToken))
            {
                string msg = string.Format("提交UFX请求[{0}]令牌失效！", functionCode);
                logger.Error(msg);
                parser.ErrorCode = ConnectionCode.ErrorLogin;

                return parser;
            }

            CT2BizMessage bizMessage = new CT2BizMessage();
            //初始化
            bizMessage.SetFunction((int)functionCode);
            bizMessage.SetPacketType(CT2tag_def.REQUEST_PACKET);

            //业务包
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();

            foreach (FieldItem item in functionItem.RequestFields)
            {
                packer.AddField(item.Name, item.Type, item.Width, item.Scale);
            }

            var dataFieldMap = UFXDataBindingHelper.GetProperty<T>();
            foreach (var request in requests)
            {
                foreach (FieldItem item in functionItem.RequestFields)
                {
                    if (dataFieldMap.ContainsKey(item.Name))
                    {
                        SetRequestField<T>(ref packer, request, item, dataFieldMap);
                    }
                    else
                    {
                        SetRequestDefaultField(ref packer, item, userToken);
                    }
                }
            }
            packer.EndPack();

#if DEBUG
            OutputParam<T>(functionCode, requests);
#endif
            unsafe
            {
                bizMessage.SetContent(packer.GetPackBuf(), packer.GetPackLen());
            }

            parser = _t2SDKWrap.SendSync2(bizMessage);
            return parser;
        }

        private void OutputParam<T>(UFXFunctionCode functionCode, List<T> requests)
        {
            FunctionItem functionItem = ConfigManager.Instance.GetFunctionConfig().GetFunctionItem(functionCode);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================");
            sb.AppendLine("FunctionCode: " + (int)functionCode);
            foreach (FieldItem item in functionItem.RequestFields)
            {
                if (item.Name.Equals("entrust_amount"))
                {
                    sb.AppendFormat("{0}|{1}|{2}|{3}\n", item.Name, UFXPackFieldType.FloatType, item.Width, item.Scale);
                }
                else
                {
                    sb.AppendFormat("{0}|{1}|{2}|{3}\n", item.Name, item.Type, item.Width, item.Scale);
                }
            }

            var dataFieldMap = UFXDataBindingHelper.GetProperty<T>();
            foreach (var request in requests)
            {
                foreach (FieldItem item in functionItem.RequestFields)
                {
                    if (dataFieldMap.ContainsKey(item.Name))
                    {
                        var dataField = dataFieldMap[item.Name];
                        Type type = request.GetType();

                        object obj = type.GetProperty(dataField.Name).GetValue(request);
                        sb.AppendFormat("{0}:{1}|", item.Name, obj);
                    }
                    else
                    {
                        switch (item.Name)
                        {
                            case "user_token":
                                {
                                    sb.AppendFormat("{0}:{1}|", item.Name, LoginManager.Instance.LoginUser.Token);
                                }
                                break;
                            default:
                                if (item.Type == UFXPackFieldType.IntType)
                                {
                                    sb.AppendFormat("{0}:{1}|", item.Name, -1);
                                }
                                else if (item.Type == UFXPackFieldType.StringType || item.Type == UFXPackFieldType.CharType)
                                {
                                    sb.AppendFormat("{0}:{1}|", item.Name, item.Name);
                                }
                                else
                                {
                                    sb.AppendFormat("{0}:{1}|", item.Name, item.Name);
                                }
                                break;
                        }
                    }
                }
                sb.Append("\n");
            }

            sb.AppendLine("=========================================================");
            logger.Info(sb.ToString());
        }

        #endregion

        #region private

        private void SetRequestField<T>(ref CT2Packer packer, T request, FieldItem fieldItem, Dictionary<string, UFXDataField> dataFieldMap)
        {
            var dataField = dataFieldMap[fieldItem.Name];
            Type type = request.GetType();
            object obj = type.GetProperty(dataField.Name).GetValue(request);
            if (fieldItem.Name.Equals("entrust_amount"))
            {
                //TODO: 委托数量字段跟文档定义不一样，文档定义是N12(整型)，但测试时传入整型出错。
                //恒生确认: 该字段将会被更新为：N16.2
                if (obj != null)
                {
                    packer.AddDouble((double)(int)obj);
                    //packer.AddInt((int)obj);
                }
                else
                {
                    packer.AddDouble(0f);
                    //packer.AddInt(0);
                }
            }
            else if (fieldItem.Type == UFXPackFieldType.IntType)
            {
                if (obj != null)
                {
                    packer.AddInt((int)obj);
                }
                else
                {
                    packer.AddInt(-1);
                }
            }
            else if (fieldItem.Type == UFXPackFieldType.FloatType)
            {
                if (obj != null)
                {
                    packer.AddDouble((double)obj);
                }
                else
                {
                    packer.AddDouble(0f);
                }
            }
            else if (fieldItem.Type == UFXPackFieldType.StringType)
            {
                if (obj != null)
                {
                    packer.AddStr(obj.ToString());
                }
                else
                {
                    packer.AddStr("");
                }
            }
            else
            {
                packer.AddStr("");
            }
        }

        private void SetRequestDefaultField(ref CT2Packer packer, FieldItem fieldItem, string userToken)
        {
            switch (fieldItem.Name)
            {
                case "user_token":
                    {
                        packer.AddStr(userToken);
                    }
                    break;
                default:
                    if (fieldItem.Type == UFXPackFieldType.IntType)
                    {
                        packer.AddInt(-1);
                    }
                    else if (fieldItem.Type == UFXPackFieldType.StringType || fieldItem.Type == UFXPackFieldType.CharType)
                    {
                        packer.AddStr(fieldItem.Name);
                    }
                    else
                    {
                        packer.AddStr(fieldItem.Name);
                    }
                    break;
            }
        }

        #endregion
    }
}
