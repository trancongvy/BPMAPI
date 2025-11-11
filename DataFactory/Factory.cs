using BPMAPI.APIControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;

namespace BPMAPI.DataFactory
{
    public static class Factory
    {
        
        public static ConcurrentDictionary<string, SingleData> LstSingle = new ConcurrentDictionary<string, SingleData>();
        public static ConcurrentDictionary<string, MTDTData> LstMtDt = new ConcurrentDictionary<string, MTDTData>();

        public static SingleData findSingle(string tableName)
        {
            if (tableName == null) return null;
            foreach (SingleData c in LstSingle.Values)
            {
                if (c == null) continue;
                if (c.TableName.ToLower() == tableName.ToLower())
                    return c;
            }
            return null;

        }
        public static SingleData findSingle(int sysTableID)
        {

            foreach (SingleData c in LstSingle.Values)
            {

                if (c.sysTableID == sysTableID.ToString())
                    return c;
            }
            return null;

        }
        public static MTDTData findMTDT(string tableName)
        {
            MTDTData data;
            if (LstMtDt.ContainsKey(tableName))
            {
                LstMtDt.TryGetValue(tableName, out data);
                return data;
            }
            else
                return null;

        }
        public static MTDTData findMTDT(int sysTableID)
        {
           
            foreach (MTDTData c in LstMtDt.Values)
            {
                if (c.sysTableID == sysTableID.ToString())
                    return c;
            }
            return null;

        }
        public static SqlDbType GetDbType(int fType)
        {
            SqlDbType tmp = SqlDbType.VarChar;
            switch (fType)
            {
                case 0:
                case 1:
                    return SqlDbType.NVarChar;

                case 2:
                case 16:
                    return SqlDbType.NVarChar;
                case 3:
                case 4:
                case 5:
                    return SqlDbType.Int;

                case 6:
                case 7:
                case 15:
                    return SqlDbType.UniqueIdentifier;

                case 8:
                    return SqlDbType.Decimal;

                case 9:
                case 11:
                case 14:
                    return SqlDbType.DateTime;

                case 10:
                    return SqlDbType.Bit;

                case 12:
                    return SqlDbType.Image;

                case 13:
                    return SqlDbType.NText;
            }
            return tmp;
        }
        
    }
    public struct SqlField
    {
        public string FieldName;
        public SqlDbType DbType;

        public SqlField(string fieldName, SqlDbType dbType)
        {
            this.FieldName = fieldName;
            this.DbType = dbType;

        }
    }
    
}