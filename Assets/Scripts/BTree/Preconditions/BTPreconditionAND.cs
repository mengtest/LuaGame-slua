﻿
namespace BTree
{
    /// <summary>
    /// Check： 子节点全部返回true时该节点才返回true，否则返回false
    /// </summary>
    public class BTPreconditionAND : BTPrecondition
    {
        private BTPrecondition[] mPreconditions;
        public BTPreconditionAND() { }
        public BTPreconditionAND(params BTPrecondition[] param)
        {
            if (param == null)
            {
                Debugger.Log("BTreeNodePreconditionAND is null");
                return;
            }
            if (param.Length == 0)
            {
                Debugger.Log("BTreeNodePreconditionAND's length is 0");
                return;
            }
            mPreconditions = param;
        }
        public override bool Check(BTInput _input)
        {
            for (int i = 0; i < mPreconditions.Length; i++)
            {
                if (!mPreconditions[i].Check(_input))
                {
                    return false;
                }
            }
            return true;
        }
        public void SetChildPrecondition(params BTPrecondition[] param)
        {
            mPreconditions = param;
        }
        public BTPrecondition[] GetChildPrecondition()
        {
            return mPreconditions;
        }
        public int GetChildPreconditionCount()
        {
            if (mPreconditions != null)
            {
                return mPreconditions.Length;
            }
            return 0;
        }
    }
}
