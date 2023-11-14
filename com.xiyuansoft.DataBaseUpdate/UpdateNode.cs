using com.xiyuansoft.bormodel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.xiyuansoft.DataBaseUpdate
{
    public class UpdateNode
    {
        static public String batabaseV = "batabaseV";

        protected string newDbv;
        public string NewDbv
        {
            get { return newDbv; }
        }

        protected string oldDbv;
        public string OldDbv
        {
            get { return oldDbv; }
        }

        private UpdateNode preUn;
        public UpdateNode PreUn
        {
            get { return preUn; }
            set { preUn = value; }
        }

        private UpdateNode nextUn;
        public UpdateNode NextUn
        {
            get { return nextUn; }
            set { nextUn = value; }
        }

        private string updateError;
        public string UpdateError
        {
            get { return updateError; }
            //set { updateError = value; }
        }
        //static public string UpdateError_target_is_Newest = "目标数据数据库已为最新版本";
        static public string UpdateError_target_is_Out = "目标数据数据库不在可更新的版本范围内";
        //static public string UpdateError_target_is_NotnewDbv = "本对象不能更新到目标版本";

        public UpdateNode(UpdateNode inPreUn)
        {
            preUn = inPreUn;
        }
        public UpdateNode(UpdateNode inPreUn, UpdateNode inNextUn)
        {
            preUn = inPreUn;
            nextUn = inNextUn;
        }
        public UpdateNode()
        {

        }

        public void doUpdate(string CurrentDbv)
        {
            if (CurrentDbv == oldDbv)
            {
                nodeUpdate();
            }
            else
            {
                UpdateNode currentVNode = searchCurrentVNode(CurrentDbv);
                if (currentVNode == null)
                {
                    throw new ApplicationException(UpdateError_target_is_Out);
                }
                currentVNode.nodeUpdate();
            }     
        }

        private void nodeUpdate()
        {
            //执行更新任务
            thidNodeUpdate();

            //写新版本号TargetDbv
            writeTargetV();

            //下一节点
            if (nextUn != null)
            {
                nextUn.nodeUpdate();
            }
        }

        private void writeTargetV()
        {
            Hashtable tHt = new Hashtable();
            tHt.Add(BizPars.fBizParValue, NewDbv);
            BizPars.getnSingInstance().updateByOneField(BizPars.fBizParName, batabaseV, tHt);
        }

        protected virtual void thidNodeUpdate()
        {
        }

        private UpdateNode searchCurrentVNode(string CurrentDbv)
        {
            if (CurrentDbv == oldDbv)
            {
                return this;
            }
            else if (preUn != null)
            {
                return preUn.searchCurrentVNode(CurrentDbv);
            }
            else
            {
                return null;
            }
        }
    }
}
