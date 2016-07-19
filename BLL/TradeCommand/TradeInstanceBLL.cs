﻿using BLL.SecurityInfo;
using DBAccess;
using DBAccess.TradeInstance;
using Model.EnumType;
using Model.SecurityInfo;
using Model.UI;
using System.Collections.Generic;

namespace BLL.TradeCommand
{
    public class TradeInstanceBLL
    {
        private TradingInstanceDAO _tradeinstdao = new TradingInstanceDAO();
        private TradingInstanceSecurityDAO _tradeinstsecudao = new TradingInstanceSecurityDAO();
        private TradeInstanceDAO _tradeinstancedao = new TradeInstanceDAO();

        public TradeInstanceBLL()
        { 
        }

        public int Create(TradingInstance tradeInstance, OpenPositionItem openItem, List<OpenPositionSecurityItem> secuItems)
        {
            var tradeinstSecus = GetTradingInstanceSecurities(tradeInstance, openItem, secuItems);
            int ret = _tradeinstancedao.Create(tradeInstance, tradeinstSecus);
            if (ret > 0)
            {
                return tradeInstance.InstanceId;
            }
            else
            {
                return -1;
            }
        }

        public int Update(TradingInstance tradeInstance, OpenPositionItem openItem, List<OpenPositionSecurityItem> secuItems)
        {
            var tradeinstSecus = GetTradingInstanceSecurities(tradeInstance, openItem, secuItems);
            return _tradeinstancedao.Update(tradeInstance, tradeinstSecus);
        }

        public TradingInstance GetInstance(int instanceId)
        {
            return _tradeinstdao.GetCombine(instanceId);
        }

        public TradingInstance GetInstance(string instanceCode)
        {
            var instance = _tradeinstdao.GetCombineByCode(instanceCode);
            return instance;
        }

        #region

        private List<TradingInstanceSecurity> GetTradingInstanceSecurities(TradingInstance tradingInstance, OpenPositionItem openItem, List<OpenPositionSecurityItem> secuItems)
        {
            List<TradingInstanceSecurity> tradeInstanceSecuItems = new List<TradingInstanceSecurity>();
            foreach (var item in secuItems)
            {
                TradingInstanceSecurity tiSecuItem = new TradingInstanceSecurity
                {
                    InstanceId = tradingInstance.InstanceId,
                    SecuCode = item.SecuCode,
                };

                var findItem = SecurityInfoManager.Instance.Get(item.SecuCode);
                if (findItem != null)
                {
                    tiSecuItem.SecuType = findItem.SecuType;
                }

                if (item.Selection)
                {
                    switch (tiSecuItem.SecuType)
                    {
                        case SecurityType.Stock:
                            {
                                tiSecuItem.InstructionPreBuy = openItem.Copies * item.WeightAmount;
                                if (item.DirectionType == EntrustDirection.BuySpot)
                                {
                                    tiSecuItem.PositionType = PositionType.StockLong;
                                }
                                else if (item.DirectionType == EntrustDirection.SellSpot)
                                {
                                    tiSecuItem.PositionType = PositionType.StockShort;
                                }
                            }
                            break;
                        case SecurityType.Futures:
                            {
                                tiSecuItem.InstructionPreSell = openItem.Copies * item.WeightAmount;
                                if (item.DirectionType == EntrustDirection.SellOpen)
                                {
                                    tiSecuItem.PositionType = PositionType.FuturesShort;
                                }
                                else if (item.DirectionType == EntrustDirection.BuyClose)
                                {
                                    tiSecuItem.PositionType = PositionType.FuturesLong;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                tradeInstanceSecuItems.Add(tiSecuItem);
            }

            return tradeInstanceSecuItems;
        }

        #endregion
    }
}
