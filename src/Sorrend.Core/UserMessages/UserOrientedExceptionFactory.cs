using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace Sorrend.Core.UserMessages
{
    public static class UserOrientedExceptionFactory
    {
        private static readonly AsyncLocal<Scope> CurrentScope = new AsyncLocal<Scope>();

        public static UserOrientedException CreateScoped(
            IFormatProvider formatProvider,
            [StructuredMessageTemplate] string messageTemplate,
            [ItemCanBeNull] params object[] arguments)
        {
            return new UserOrientedException(
                formatProvider,
                FormatMessageTemplate(messageTemplate),
                IncludeScopeArguments(arguments));
        }

        public static UserOrientedException CreateScoped(
            Exception innerException,
            IFormatProvider formatProvider,
            string messageTemplate,
            [ItemCanBeNull] params object[] arguments)
        {
            return new UserOrientedException(
                innerException,
                formatProvider,
                FormatMessageTemplate(messageTemplate),
                IncludeScopeArguments(arguments));
        }

        private static string FormatMessageTemplate(string messageTemplate)
        {
            var result = new StringBuilder(messageTemplate);

            if (result[result.Length - 1] != '.')
            {
                result.Append('.');
            }

            var innermostScope = CurrentScope.Value;
            if (innermostScope == null)
            {
                return result.ToString();
            }

            result.Append(" (");

            for (
                var scope = innermostScope;
                scope != null;
                scope = scope.Parent)
            {
                if (scope != innermostScope)
                {
                    result.Append(' ');
                }

                result.Append(scope.MessageTemplate);

                if (result[result.Length - 1] != '.')
                {
                    result.Append('.');
                }
            }

            result.Append(')');

            return result.ToString();
        }

        private static object[] IncludeScopeArguments(IEnumerable<object> arguments)
        {
            var result = new List<object>(arguments);

            for (
                var scope = CurrentScope.Value;
                scope != null;
                scope = scope.Parent)
            {
                result.AddRange(scope.Arguments);
            }

            return result.ToArray();
        }

        public static IDisposable BeginScope(
            [StructuredMessageTemplate] string messageTemplate,
            [ItemCanBeNull] params object[] arguments)
        {
            return new Scope(messageTemplate, arguments);
        }

        private sealed class Scope : IDisposable
        {
            public string MessageTemplate { get; }

            public object[] Arguments { get; }

            [CanBeNull]
            public Scope Parent { get; } = CurrentScope.Value;

            public Scope(string messageTemplate, [ItemCanBeNull] object[] arguments)
            {
                MessageTemplate = messageTemplate;
                Arguments = arguments;
            }

            public void Dispose()
            {
                CurrentScope.Value = Parent;
            }
        }
    }
}
