﻿using Scribi.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scribi.CodeGeneration
{
    /// <summary>
    /// TODO How to call Signal R  Client methods from user code!!!???
    /// </summary>
    public static class HubCreator
    {
        #region Controller Creation
        private const string HubTemplate =
        @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Scribi.Interfaces;

namespace Scribi.Hubs
{{
    [HubName(""{0}"")]
    public class {0}Hub : Hub{3}
    {{
        private readonly {2} _obj;

        public {0}Hub({2} obj)
        {{
            _obj = obj;
        }}

{1}
    }}
}}
        ";

        private const string HubMethod =
        @"
        public {0} {1} ({2})
        {{
            return _obj.{1}({3});
        }}
        ";

        private const string VoidHubMethod =
@"
        public void {1} ({2})
        {{
            _obj.{1}({3});
        }}
        ";
        #endregion


        public static Tuple<string,string> Create(Type type, ScriptUnitAttribute attr)
        {
            var sb = new StringBuilder();
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes<HubMethodAttribute>();
                if (attributes.Any())
                {
                    foreach (var attribute in attributes)
                    {
                        var parameters = method.GetParameters();
                        sb.Append(string.Format(method.ReturnType == typeof(void) ? VoidHubMethod : HubMethod,
                                                method.ReturnType,
                                                method.Name,
                                                parameters.Any() ? ParametersToParameters(parameters) : string.Empty,
                                                parameters.Any() ? ParametersToCallParams(parameters) : string.Empty));
                        sb.AppendLine();
                    }
                }
            }
            return new Tuple<string,string>(attr.Name + "Hub", string.Format(HubTemplate, attr.Name, sb.ToString(), type, attr.ClientInterface != null ? $"<{attr.ClientInterface}>" : ""));
        }

        private static string ParametersToParameters(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.ParameterType);
                sb.Append(" ");
                sb.Append(param.Name);
            }
            return sb.ToString();
        }

        private static string ParametersToCallParams(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.Name);
            }
            return sb.ToString();
        }
    }
}
