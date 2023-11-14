using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;

namespace com.xiyuansoft.DataBasePro
{
    public interface ICreateTable
    {
        void CreateTable(DbService dbAccess);
        void CreateTable(DbService dbAccess,ArrayList allCreateObjs);
    }
}
