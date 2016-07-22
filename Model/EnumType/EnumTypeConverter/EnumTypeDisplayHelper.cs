﻿using Model.SecurityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.EnumType.EnumTypeConverter
{
    public class EnumTypeDisplayHelper
    {
        public static string GetExecuteType(ExecuteType executeType)
        {
            string type = string.Empty;

            switch (executeType)
            {
                case ExecuteType.OpenPosition:
                    {
                        type = "开仓";
                    }
                    break;
                case ExecuteType.ClosePosition:
                    {
                        type = "平仓";
                    }
                    break;
                case ExecuteType.AdjustPosition:
                    {
                        type = "调仓";
                    }
                    break;
                default:
                    break;
            }

            return type;
        }

        public static string GetEntrustDirection(EntrustDirection entrustDirection)
        {
            string direction = string.Empty;

            switch (entrustDirection)
            {
                case EntrustDirection.BuySpot:
                    {
                        direction = "买入";
                    }
                    break;
                case EntrustDirection.SellSpot:
                    {
                        direction = "卖出";
                    }
                    break;
                case EntrustDirection.SellOpen:
                    {
                        direction = "卖出开仓";
                    }
                    break;
                case EntrustDirection.BuyClose:
                    {
                        direction = "买入平仓";
                    }
                    break;
                case EntrustDirection.Buy:
                    {
                        direction = "买入";
                    }
                    break;
                case EntrustDirection.Sell:
                    {
                        direction = "卖出";
                    }
                    break;
                case EntrustDirection.AdjustedToBuySell:
                    {
                        direction = "调整到[买卖]";
                    }
                    break;
                default:
                    break;
            }

            return direction;
        }

        public static string GetLongShort(SecurityType securityType)
        {
            string ret = string.Empty;
            
            switch (securityType)
            {
                case SecurityType.Stock:
                    ret = "多头";
                    break;
                case SecurityType.Futures:
                    ret = "空头";
                    break;
                default:
                    break;
            }

            return ret;
        }

        public static string GetPriceType(PriceType priceType)
        {
            string priceTypeName = string.Empty;

            switch (priceType)
            { 
                case PriceType.Market:
                    priceTypeName = "市价";
                    break;
                case PriceType.Assign:
                    priceTypeName = "指定价";
                    break;
                case PriceType.Last:
                    priceTypeName = "最新价";
                    break;
                case PriceType.Automatic:
                    priceTypeName = "自动盘口";
                    break;
                case PriceType.Arbitrary:
                    priceTypeName = "任意价";
                    break;
                case PriceType.Sell1:
                    priceTypeName = "卖一价";
                    break;
                case PriceType.Sell2:
                    priceTypeName = "卖二价";
                    break;
                case PriceType.Sell3:
                    priceTypeName = "卖三价";
                    break;
                case PriceType.Sell4:
                    priceTypeName = "卖四价";
                    break;
                case PriceType.Sell5:
                    priceTypeName = "卖五价";
                    break;
                case PriceType.Sell6:
                    priceTypeName = "卖六价";
                    break;
                case PriceType.Sell7:
                    priceTypeName = "卖七价";
                    break;
                case PriceType.Sell8:
                    priceTypeName = "卖八价";
                    break;
                case PriceType.Sell9:
                    priceTypeName = "卖九价";
                    break;
                case PriceType.Sell10:
                    priceTypeName = "卖十价";
                    break;
                case PriceType.Buy1:
                    priceTypeName = "买一价";
                    break;
                case PriceType.Buy2:
                    priceTypeName = "买二价";
                    break;
                case PriceType.Buy3:
                    priceTypeName = "买三价";
                    break;
                case PriceType.Buy4:
                    priceTypeName = "买四价";
                    break;
                case PriceType.Buy5:
                    priceTypeName = "买五价";
                    break;
                case PriceType.Buy6:
                    priceTypeName = "买六价";
                    break;
                case PriceType.Buy7:
                    priceTypeName = "买七价";
                    break;
                case PriceType.Buy8:
                    priceTypeName = "买八价";
                    break;
                case PriceType.Buy9:
                    priceTypeName = "买九价";
                    break;
                case PriceType.Buy10:
                    priceTypeName = "买十价";
                    break;
                default:
                    break;
            }

            return priceTypeName;
        }

        public static string GetEntrustPriceType(EntrustPriceType entrustPriceType)
        {
            string entrustPriceTypeName = string.Empty;

            switch (entrustPriceType)
            { 
                case EntrustPriceType.FixedPrice:
                    entrustPriceTypeName = "限价";
                    break;
                case EntrustPriceType.FifthIsLeftOffSH:
                    entrustPriceTypeName = "五档即成剩撤";
                    break;
                case EntrustPriceType.FifthIsLeftOffSZ:
                    entrustPriceTypeName = "五档即成剩撤";
                    break;
                case EntrustPriceType.FifthIsLeftTurnSH:
                    entrustPriceTypeName = "五档即成剩转";
                    break;
                case EntrustPriceType.LeftTurnSZ:
                    entrustPriceTypeName = "五档即成剩转";
                    break;
                case EntrustPriceType.OppSideOptSZ:
                    entrustPriceTypeName = "对手方最优";
                    break;
                case EntrustPriceType.BestBestSZ:
                    entrustPriceTypeName = "本方最优";
                    break;
                case EntrustPriceType.FOKMarketSZ:
                    entrustPriceTypeName = "全额成或撤";
                    break;
                case EntrustPriceType.FifthIsLeftOffCFX:
                    entrustPriceTypeName = "五档即成剩撤";
                    break;
                case EntrustPriceType.FifthIsLeftTurnCFX:
                    entrustPriceTypeName = "五档即成剩转";
                    break;
                case EntrustPriceType.BestOneLeftOff:
                    entrustPriceTypeName = "最优一档即成剩撤";
                    break;
                case EntrustPriceType.BestOneLeftTurn:
                    entrustPriceTypeName = "最优一档即成剩转";
                    break;
                default:
                    break;
            }

            return entrustPriceTypeName;
        }
    }
}
