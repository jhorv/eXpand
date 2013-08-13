﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.Persistent.Base;
using Xpand.Persistent.Base.Logic;
using Xpand.Persistent.Base.Logic.Model;
using Xpand.Persistent.Base.Logic.NodeGenerators;
using Xpand.Utils;
using Xpand.Utils.Helpers;
using System.Linq;

namespace Xpand.ExpressApp.Logic.NodeUpdaters {
    public abstract class LogicRulesNodeUpdater<TLogicRule, TModelLogicRule> :
        ModelNodesGeneratorUpdater<LogicRulesNodesGenerator>
        where TLogicRule : ILogicRule
        where TModelLogicRule : IModelLogicRule{
        IEnumerable<PropertyInfo> _explicitProperties;

        void AddRules(ModelNode node, IEnumerable<TLogicRule> attributes, IModelClass modelClass) {
            foreach (TLogicRule attribute in attributes) {
                var rule = node.AddNode<TModelLogicRule>(attribute.Id);
                SetAttribute(rule, attribute);
                ((IModelNode)rule).Index = attribute.Index;
                rule.ModelClass = modelClass;
                rule.TypeInfo = modelClass.TypeInfo;
                ConvertModelNodes(attribute, rule);
            }
        }

        void ConvertModelNodes(TLogicRule attribute, TModelLogicRule rule) {
            if (_explicitProperties == null)
                _explicitProperties = XpandReflectionHelper.GetExplicitProperties(attribute.GetType());
            foreach (PropertyInfo explicitProperty in _explicitProperties) {
                object[] customAttributes = explicitProperty.GetCustomAttributes(typeof(TypeConverterAttribute), false);
                if (customAttributes.Length > 0) {
                    var converter = (TypeConverter)ReflectionHelper.CreateObject(Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName), new object[] { rule.Application });
                    string name = explicitProperty.Name.Substring(explicitProperty.Name.LastIndexOf(".", StringComparison.Ordinal) + 1);
                    PropertyInfo propertyInfo = attribute.GetType().GetProperty(name);
                    object value = propertyInfo.GetValue(attribute, null);
                    if (value != null) {
                        object convertTo = converter.ConvertTo(value, rule.TypeInfo.Type);
                        explicitProperty.SetValue(attribute, convertTo, null);
                    }
                }
            }
        }

        protected abstract void SetAttribute(TModelLogicRule rule, TLogicRule attribute);

        public override void UpdateNode(ModelNode node) {
            var propertyName = ReflectionExtensions.GetPropertyName(ExecuteExpression());
            if (node.Parent.Id == propertyName) {
                foreach (IModelClass modelClass in node.Application.BOModel) {
                    var findAttributes = LogicRuleManager.FindAttributes(modelClass.TypeInfo);
                    AddRules(node, findAttributes.OfType<TLogicRule>(), modelClass);
                }
            }
        }

        protected abstract Expression<Func<IModelApplication, object>> ExecuteExpression();
    }
}