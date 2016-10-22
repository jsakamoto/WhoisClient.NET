using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public static class TestContextExtensionsForAsync
    {
        private static Type GetTestClass(TestContext context)
        {
            return Type.GetType(context.FullyQualifiedTestClassName, throwOnError: true);
        }

        private static IEnumerable<object[]> GetParameters(Type classType, string methodName)
        {
            var method = classType.GetMethod(methodName);

            var testCase = method
                .GetCustomAttributes(typeof(TestCaseAttribute), false)
                .Cast<TestCaseAttribute>()
                .Select(x => x.Parameters);

            var testCaseSource = method
                .GetCustomAttributes(typeof(TestCaseSourceAttribute), false)
                .Cast<TestCaseSourceAttribute>()
                .SelectMany(x =>
                {
                    var p = classType.GetProperty(x.SourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    var val = (p != null)
                        ? p.GetValue(null, null)
                        : classType.GetField(x.SourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);

                    return ((object[])val).Cast<object[]>();
                });

            return testCase.Concat(testCaseSource);
        }

        /// <summary>Run Parameterized Test marked by TestCase Attribute.</summary>
        public static async Task RunAsync<T1, T2>(this TestContext context, Func<T1, T2, Task> assertion)
        {
            var type = GetTestClass(context);
            foreach (var parameters in GetParameters(type, context.TestName))
            {
                await assertion(
                    (T1)parameters[0],
                    (T2)parameters[1]
                    );
            }
        }

        /// <summary>Run Parameterized Test marked by TestCase Attribute.</summary>
        public static async Task RunAsync<T1, T2, T3>(this TestContext context, Func<T1, T2, T3, Task> assertion)
        {
            var type = GetTestClass(context);
            foreach (var parameters in GetParameters(type, context.TestName))
            {
                await assertion(
                    (T1)parameters[0],
                    (T2)parameters[1],
                    (T3)parameters[2]
                    );
            }
        }
    }
}
