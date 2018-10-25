﻿

using System;
using System.Collections.Generic;
namespace BTree
{
    public class BTFactory
    {
        public delegate void BTTypeRegister(Action<Type> action);
        public static event BTTypeRegister onRegisterAction;
        public static event BTTypeRegister onRegisterPrecondition;


        private static bool mInited = false;

        private static Dictionary<string, Type> mPreconditionTypeDic = null;
        public static Dictionary<string, Type> PreconditionTypeDic
        {
            get
            {
                if (mPreconditionTypeDic == null)
                {
                    Init();
                }
                return mPreconditionTypeDic;
            }
        }
        private static Dictionary<string, Type> mActionTypeDic = null;
        public static Dictionary<string, Type> ActionTypeDic
        {
            get
            {
                if (mActionTypeDic == null)
                {
                    Init();
                }
                return mActionTypeDic;
            }
        }
        public static void AddActionType(Type type)
        {
            if (mActionTypeDic.ContainsKey(type.Name))
            {
                mActionTypeDic[type.Name] = type;
            }
            else
            {
                mActionTypeDic.Add(type.Name, type);
            }
        }
        public static void AddPreconditionType(Type type)
        {
            if (mPreconditionTypeDic.ContainsKey(type.Name))
            {
                mPreconditionTypeDic[type.Name] = type;
            }
            else
            {
                mPreconditionTypeDic.Add(type.Name, type);
            }
        }
        public static void Init()
        {
            if(mInited)
            {
                return;
            }
            mInited = true;

            mActionTypeDic = new Dictionary<string, Type>();
            mPreconditionTypeDic = new Dictionary<string, Type>();

            if (onRegisterAction != null)
            {
                onRegisterAction(AddActionType);
            }
            if (onRegisterPrecondition != null)
            {
                onRegisterPrecondition(AddPreconditionType);
            }
        }

        #region 从配置生成行为树相关方法
        public static BTNode CreateBTreeRootFromConfig(TreeConfig _config)
        {
            Init();
            BTNode[] _nodes = CreateBTreeFromConfig(_config);
            return _nodes[0];
        }
        public static BTNode[] CreateBTreeFromConfig(TreeConfig _config)
        {
            Init();
            BTNode[] _nodes = new BTNode[_config.m_Nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i] = null;
            }
            for (int i = 0; i < _config.m_Nodes.Length; i++)
            {
                if (_nodes[i] == null)
                {
                    _nodes[i] = CreateTreeNode(ref _nodes, _config.m_Nodes, i);
                }
                if (_config.m_Nodes[i].m_Preconditions != null)
                {
                    var precondition = CreatePreconditionFromConfig(_config.m_Nodes[i]);
                    //Debugger.Log(precondition);
                    _nodes[i].SetPrecondition(precondition);
                }
            }
            return _nodes;
        }
        private static BTNode CreateTreeNode(ref BTNode[] _nodes, TreeNodeConfig[] _nodeConfigs, int index)
        {
            TreeNodeConfig _nodeConfig = _nodeConfigs[index];
            if (_nodeConfig.m_ParentIndex >= 0 && _nodes[_nodeConfig.m_ParentIndex]==null)
            {
                _nodes[_nodeConfig.m_ParentIndex] = CreateTreeNode(ref _nodes, _nodeConfigs, _nodeConfig.m_ParentIndex);
            }
            BTNode parent = null;
            if (_nodeConfig.m_ParentIndex >= 0)
            {
                parent = _nodes[_nodeConfig.m_ParentIndex];
            }
            NodeType type = (NodeType)_nodeConfig.m_NodeType;
            BTNode _node = null;
            switch (type)
            {
                case NodeType.SelectorNode:
                    SelectorNodeType subType = (SelectorNodeType)_nodeConfig.m_NodeSubType;
                    _node = CreateSelectorNode(subType, parent, _nodeConfig.m_NodeName, _nodeConfig.m_OtherParams);
                    break;
                case NodeType.ActionNode:
                    _node = CreateActionNode(parent, _nodeConfig.m_NodeName, ActionTypeDic[_nodeConfig.m_ActionNodeName]);
                    break;
                default:
                    break;
            }
            _node.index = index;
            return _node;
        }
        private static BTNode CreateSelectorNode(SelectorNodeType _subType, BTNode _parent, string _nodeName, params int[] _param)
        {
            BTNode _node = null;
            switch (_subType)
            {
                case SelectorNodeType.BTParallel:
                    _node = CreateParallelNode(_parent, _nodeName, (BTParallelCondition)_param[0]);
                    break;
                case SelectorNodeType.BTPrioritySelector:
                    _node = CreatePrioritySelectorNode(_parent, _nodeName);
                    break;
                case SelectorNodeType.BTNonePrioritySelector:
                    _node = CreateNonePrioritySelectorNode(_parent, _nodeName);
                    break;
                case SelectorNodeType.BTSequence:
                    _node = CreateSequenceNode(_parent, _nodeName);
                    break;
                case SelectorNodeType.BTLoop:
                    _node = CreateLoopNode(_parent, _nodeName, _param[0]);
                    break;
                default:
                    break;
            }

            return _node;
        }
        private static BTNode CreateActionNode(BTNode _parent, string _nodeName, Type type)
        {
            BTNode node = (BTNode)type.GetConstructor(new Type[] { typeof(BTNode) }).Invoke(new object[] { _parent });
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }
        private static BTPrecondition CreatePreconditionFromConfig(TreeNodeConfig _nodeConfig)
        {
            PreconditionConfig[] _condConfigs = _nodeConfig.m_Preconditions;
            BTPrecondition[] _nodePreconditions = new BTPrecondition[_condConfigs.Length];
            for (int i = 0; i < _nodePreconditions.Length; i++)
            {
                _nodePreconditions[i] = null;
            }
            for (int i = 0; i < _nodePreconditions.Length; i++)
            {
                if (_nodePreconditions[i] == null)
                {
                    _nodePreconditions[i] = CreatePrecondition(ref _nodePreconditions, _condConfigs, i);
                }
            }
            return _nodePreconditions[0];
        }
        private static BTPrecondition CreatePrecondition(ref BTPrecondition[] _nodePreconditions, PreconditionConfig[] _condConfigs, int _index)
        {
            int[] _childIndexs = _condConfigs[_index].m_ChildIndexs;
            //int _parentIndex = _condConfigs[_index].m_ParentIndex;
            if (_childIndexs != null && _childIndexs.Length != 0)
            {
                for (int i = 0; i < _childIndexs.Length; i++)
                {
                    if (_nodePreconditions[_childIndexs[i]] == null)
                    {
                        _nodePreconditions[_childIndexs[i]] = CreatePrecondition(ref _nodePreconditions, _condConfigs, _childIndexs[i]);
                    }
                }
            }
            BTPrecondition _precondition = null;
            if (_childIndexs != null && _childIndexs.Length > 0)
            {
                PreconditionType type = (PreconditionType)_condConfigs[_index].m_Type;
                BTPrecondition[] _childNodePreconditions = new BTPrecondition[_childIndexs.Length];
                for (int i = 0; i < _childIndexs.Length; i++)
                {
                    _childNodePreconditions[i] = _nodePreconditions[_childIndexs[i]];
                }
                switch (type)
                {
                    case PreconditionType.And:
                        _precondition = new BTPreconditionAND(_childNodePreconditions);
                        break;
                    case PreconditionType.Or:
                        _precondition = new BTPreconditionOR(_childNodePreconditions);
                        break;
                    case PreconditionType.Not:
                        _precondition = new BTPreconditionNOT(_childNodePreconditions[0]);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                string typeName = _condConfigs[_index].m_PreconditionName;
                if (PreconditionTypeDic.ContainsKey(typeName))
                {
                    Type type = PreconditionTypeDic[typeName];
                    _precondition = (BTPrecondition)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
                }
            }
            
            return _precondition;
        }
        #endregion
        #region 从行为树生成配置方法
        public static TreeConfig CreateConfigFromBTreeRoot(BTNode _root)
        {
            TreeConfig _tree = new TreeConfig();
            _tree.m_Name = _root.name;
            int _nodeCount = GetBTreeChildNodeNum(_root) + 1;
            _tree.m_Nodes = new TreeNodeConfig[_nodeCount];
            int index = 0;
            GetTreeNodeConfigFromBTreeRoot(_root, ref _tree.m_Nodes, ref index, -1);
            return _tree;
        }
        private static void GetTreeNodeConfigFromBTreeRoot(BTNode _root, ref TreeNodeConfig[] _treeNodeList, ref int _index, int _parentIndex)
        {
            _treeNodeList[_index] = new TreeNodeConfig();
            _treeNodeList[_index].m_NodeName = _root.name;
            _treeNodeList[_index].m_ParentIndex = _parentIndex;
            bool _isAction = _root.isAcitonNode;
            if (_isAction)
            {
                _treeNodeList[_index].m_NodeType = (int)NodeType.ActionNode;
            }
            else
            {
                _treeNodeList[_index].m_NodeType = (int)NodeType.SelectorNode;
            }
            _treeNodeList[_index].m_ActionNodeName = _isAction ? _root.GetType().Name : null;
            _treeNodeList[_index].m_NodeSubType = !_isAction ? (int)(Enum.Parse(typeof(SelectorNodeType), _root.GetType().Name)) : 0;
            _treeNodeList[_index].m_OtherParams = GetOtherParamsFromBTreeNode(_root, (NodeType)_treeNodeList[_index].m_NodeType, (SelectorNodeType)_treeNodeList[_index].m_NodeSubType);
            if (_root.precondition != null)
            {
                int _preconditionCount = GetBTreeChildPreconditionNum(_root.precondition) + 1;
                _treeNodeList[_index].m_Preconditions = new PreconditionConfig[_preconditionCount];
                int index = 0;
                GetPreconditionConfigFromBtreeNode(_root.precondition, ref _treeNodeList[_index].m_Preconditions, ref index, -1);
            }
            int parentIndex = _index;
            for (int i = 0; i < _root.childCount; i++)
            {
                _index = _index + 1;
                GetTreeNodeConfigFromBTreeRoot(_root.children[i], ref _treeNodeList, ref _index, parentIndex);
            }
        }
        private static void GetPreconditionConfigFromBtreeNode(BTPrecondition _precondition, ref PreconditionConfig[] _preconditionList, ref int _index, int _parentIndex = -1)
        {
            _preconditionList[_index] = new PreconditionConfig();
            _preconditionList[_index].m_ParentIndex = _parentIndex;
            Type type = _precondition.GetType();
            _preconditionList[_index].m_PreconditionName = type.Name.Split('`')[0];
            if (type.Equals(typeof(BTPreconditionAND)))
            {
                _preconditionList[_index].m_Type = (int)PreconditionType.And;
                BTPrecondition[] _childPreconditon = ((BTPreconditionAND)_precondition).GetChildPrecondition();
                _preconditionList[_index].m_ChildIndexs = new int[_childPreconditon.Length];
                int parentIndex = _index;
                for (int i = 0; i < _childPreconditon.Length; i++)
                {
                    _index = _index + 1;
                    _preconditionList[parentIndex].m_ChildIndexs[i] = _index;
                    GetPreconditionConfigFromBtreeNode(_childPreconditon[i], ref _preconditionList, ref _index, parentIndex);
                }
            }
            else if (type.Equals(typeof(BTPreconditionOR)))
            {
                _preconditionList[_index].m_Type = (int)PreconditionType.Or;
                BTPrecondition[] _childPreconditon = ((BTPreconditionOR)_precondition).GetChildPrecondition();
                _preconditionList[_index].m_ChildIndexs = new int[_childPreconditon.Length];
                int parentIndex = _index;
                for (int i = 0; i < _childPreconditon.Length; i++)
                {
                    _index = _index + 1;
                    _preconditionList[parentIndex].m_ChildIndexs[i] = _index;
                    GetPreconditionConfigFromBtreeNode(_childPreconditon[i], ref _preconditionList, ref _index, parentIndex);
                }
            }
            else if (type.Equals(typeof(BTPreconditionNOT)))
            {
                _preconditionList[_index].m_Type = (int)PreconditionType.Not;
                BTPrecondition _childPreconditon = ((BTPreconditionNOT)_precondition).GetChildPrecondition();
                _preconditionList[_index].m_ChildIndexs = new int[1];
                _preconditionList[_index].m_ChildIndexs[0] = _index + 1;
                int parentIndex = _index;
                _index = _index + 1;
                GetPreconditionConfigFromBtreeNode(_childPreconditon, ref _preconditionList, ref _index, parentIndex);
            }
        }
        private static int GetBTreeChildPreconditionNum(BTPrecondition _precondition)
        {
            if (_precondition == null)
            {
                return 0;
            }
            int _count = 0;
            Type type = _precondition.GetType();
            if (type.Equals(typeof(BTPreconditionAND)))
            {
                _count += ((BTPreconditionAND)_precondition).GetChildPreconditionCount();
                BTPrecondition[] _chlidList = ((BTPreconditionAND)_precondition).GetChildPrecondition();
                if (_chlidList != null)
                {
                    for (int i = 0; i < _chlidList.Length; i++)
                    {
                        _count += GetBTreeChildPreconditionNum(_chlidList[i]);
                    }
                }
            }
            else if (type.Equals(typeof(BTPreconditionOR)))
            {
                _count += ((BTPreconditionOR)_precondition).GetChildPreconditionCount();
                BTPrecondition[] _chlidList = ((BTPreconditionOR)_precondition).GetChildPrecondition();
                if (_chlidList != null)
                {
                    for (int i = 0; i < _chlidList.Length; i++)
                    {
                        _count += GetBTreeChildPreconditionNum(_chlidList[i]);
                    }
                }
            }
            else if (type.Equals(typeof(BTPreconditionNOT)))
            {
                _count += 1;
                _count += GetBTreeChildPreconditionNum(((BTPreconditionNOT
                    )_precondition).GetChildPrecondition());
            }
            return _count;
        }
        private static int[] GetOtherParamsFromBTreeNode(BTNode _node, NodeType _nodeType, SelectorNodeType _subType)
        {
            int[] _otherParams = null;
            switch (_nodeType)
            {
                case NodeType.SelectorNode:
                    switch (_subType)
                    {
                        case SelectorNodeType.BTParallel:
                            _otherParams = new int[1];
                            _otherParams[0] = (int)((BTParallel)_node).mFinishCondition;
                            break;
                        case SelectorNodeType.BTLoop:
                            _otherParams = new int[1];
                            _otherParams[0] = ((BTLoop)_node).GetLoopCount();
                            break;
                        default:
                            break;
                    }
                    break;
                case NodeType.ActionNode:
                    _otherParams = null;
                    break;
                default:
                    break;
            }
            return _otherParams;
        }
        private static int GetBTreeChildNodeNum(BTNode _root)
        {
            int _count = _root.childCount;
            for (int i = 0; i < _root.children.Count; i++)
            {
                if (_root.children[i].childCount != 0)
                {
                    _count += GetBTreeChildNodeNum(_root.children[i]);
                }
            }
            return _count;
        }
        #endregion

        public static BTNode CreateParallelNode(BTNode _parent, string _nodeName, BTParallelCondition _conditionType)
        {
            BTParallel node = new BTParallel(_parent);
            node.mFinishCondition = _conditionType;
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }

        public static BTNode CreatePrioritySelectorNode(BTNode _parent, string _nodeName)
        {
            BTPrioritySelector node = new BTPrioritySelector(_parent);
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }

        public static BTNode CreateNonePrioritySelectorNode(BTNode _parent, string _nodeName)
        {
            BTNonePrioritySelector node = new BTNonePrioritySelector(_parent);
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }

        public static BTNode CreateSequenceNode(BTNode _parent, string _nodeName)
        {
            BTSequence node = new BTSequence(_parent);
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }

        public static BTNode CreateLoopNode(BTNode _parent, string _nodeName, int _loopCount)
        {
            BTLoop node = new BTLoop(_parent, null, _loopCount);
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }
        
        public static BTNode CreateActionNode<F>(BTNode _parent, string _nodeName) where F: BTAction
        {
            F node = (F)Activator.CreateInstance(typeof(F), _parent);
            CreateNodeCommon(node, _parent, _nodeName);
            return node;
        }
        private static void CreateNodeCommon(BTNode _node, BTNode _parent, string _nodeName)
        {
            if (_parent != null)
            {
                _parent.AddChild(_node);
            }
            _node.name = _nodeName;
        }
    }
    
    
}