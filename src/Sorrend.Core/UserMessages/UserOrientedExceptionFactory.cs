using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace Sorrend.Core.UserMessages
{
    public static class UserOrientedExceptionFactory
    {
        private static readonly AsyncLocal<Scope?> CurrentScope = new();

        public static UserOrientedException CreateScoped(
            IFormatProvider formatProvider,
            [StructuredMessageTemplate] string messageTemplate,
            params object?[] arguments)
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
            params object?[] arguments)
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

            if (result[^1] != '.')
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

                if (result[^1] != '.')
                {
                    result.Append('.');
                }
            }

            result.Append(')');

            return result.ToString();
        }

        private static object?[] IncludeScopeArguments(IEnumerable<object?> arguments)
        {
            var result = new List<object?>(arguments);

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
            params object?[] arguments)
        {
            return new Scope(messageTemplate, arguments);
        }

        private sealed record Scope(string MessageTemplate, object?[] Arguments) : IDisposable
        {
            public Scope? Parent { get; } = CurrentScope.Value;

            public void Dispose()
            {
                CurrentScope.Value = Parent;
            }
        }
    }
}
