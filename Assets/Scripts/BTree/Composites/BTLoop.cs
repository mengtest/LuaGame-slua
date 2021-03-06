﻿
namespace BTree
{
    /// <summary>
    /// class:      循环节点
    /// Evaluate:   预设的循环次数到了就返回False，否则，只调用第一个子节点的Evaluate方法，用它所返回的值作为自身的值返回
    /// Tick:       只调用第一个节点的Tick方法，若返回运行结束，则看是否需要重复运行，若循环次数没到，则自身返回运行中，若循环次数已到，则返回运行结束
    /// </summary>
    public class BTLoop : BTComposite
    {
        public int loopCount;
        private int mCurrentCount;
        private const int INFINITELOOP = -1;
        public BTLoop()
            : base()
        {
            maxChild = 1;
        }
        public BTLoop(BTNode _parentNode, BTPrecondition _precondition = null, int _loopCount = INFINITELOOP)
            : base(_parentNode, _precondition)
        {
            loopCount = _loopCount;
            mCurrentCount = 0;
            maxChild = 1;
        }

        protected override bool OnEvaluate(BTInput _input)
        {
            bool checkLoopCount = loopCount == INFINITELOOP || mCurrentCount < loopCount;
            if (!checkLoopCount)
            {
                return false;
            }
            if (CheckIndex(0))
            {
                BTNode node = mChildren[0];
                if (node.Evaluate(_input))
                {
                    return true;
                }
            }
            return false;
        }
        protected override void OnTransition(BTInput _input)
        {
            if (CheckIndex(0))
            {
                BTNode node = mChildren[0];
                node.Transition(_input);
            }
            mCurrentCount = 0;
        }
        protected override BTResult OnTick(ref BTInput _input)
        {
            BTResult result = BTResult.Success;
            if (CheckIndex(0))
            {
                BTNode node = mChildren[0];
                result = node.Tick(ref _input);

                if (result == BTResult.Success)
                {
                    if (loopCount != INFINITELOOP)
                    {
                        mCurrentCount++;
                        if (mCurrentCount == loopCount)
                        {
                            result = BTResult.Executing;
                        }
                    }
                    else
                    {
                        result = BTResult.Executing;
                    }
                }
            }
            if (result == BTResult.Success)
            {
                mCurrentCount = 0;
            }
            return result;
        }
        public int GetLoopCount()
        {
            return loopCount;
        }
    }
}
