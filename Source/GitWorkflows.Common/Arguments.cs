using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GitWorkflows.Common
{
    /// <summary>
    /// Utility methods for argument validation.
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Information about the exception constructor used by <see cref="EnsureNotNull"/> when
        /// throwing an exception.
        /// </summary>
        private static readonly ConstructorInfo _argumentNullExceptionConstructor = typeof(ArgumentNullException).GetConstructor(new[]{typeof(string)});

        /// <summary>
        /// Maps types into delegates that check whether a property of an object is <c>null</c>.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Action<object>> _nullChecks = new ConcurrentDictionary<Type, Action<object>>();

        /// <summary>
        /// Throws <see cref="ArgumentNullException"/> if any property of the given object is 
        /// <c>null</c>.
        /// </summary>
        /// 
        /// <param name="arguments">Object whose properties to validate.</param>
        /// 
        /// <exception cref="InvalidOperationException">The object has no properties.</exception>
        /// <exception cref="ArgumentNullException">A property of the object is <c>null</c>.
        /// </exception>
        /// 
        /// <exception cref="NotSupportedException">The object is not of anonymous type. A common
        /// error is to use <see cref="EnsureNotNull"/> with an actual parameter, instead an object
        /// with property corresponding to the parameter. For example:
        ///     <code>
        ///         void MyMethod(object obj)
        ///         {
        ///             Arguments.EnsureNotNull(obj);   // Throws NotSupportedException
        /// 
        ///             // Instead, this should be
        ///             Arguments.EnsureNotNull( new {obj} );
        ///         }
        ///     </code>
        /// </exception>
        /// 
        /// <remarks>
        ///     <para>This is a convenience method, intended to be used in methods which do not
        ///     allow some of their arguments to be <c>null</c>. The <paramref name="arguments"/>
        ///     parameter will usually be an anonymous object with properties corresponding to the
        ///     arguments that must not be <c>null</c>.</para>
        /// 
        ///     <example>
        ///         This method will throw <see cref="ArgumentNullException"/> if 
        ///         <c>requiredString</c> or <c>requiredList</c> is <c>null</c>.
        /// 
        ///         <code>
        ///             void MyMethod(string requiredString, IList requiredList, object optionalObject)
        ///             {
        ///                 Arguments.EnsureNotNull( new { requiredString, requiredList } );
        ///             }
        ///         </code>
        ///     </example>
        /// 
        ///     <para>EnsureNotNull method is almost as fast as hand-coded set of checks. This is
        ///     because it uses reflection only the first time it is invoked on a type, and then
        ///     generates a lambda function that will be used for checking objects for <c>null</c>
        ///     properties. This function is exactly equivalent to a hand-coded set of checks, and
        ///     there is only a small overhead of retrieving the function from cache and invoking
        ///     it.</para>
        /// 
        ///     <para>This method is thread-safe.</para>
        /// </remarks>
        public static void EnsureNotNull(object arguments)
        {
            Debug.Assert(arguments != null, "Object with arguments is null");
            var check = _nullChecks.GetOrAdd(arguments.GetType(), CreateNullValidator);
            check(arguments);
        }

        /// <summary>
        /// Creates the lambda function that throws if any property on objects of the given type is 
        /// <c>null</c>.
        /// </summary>
        /// 
        /// <param name="type">The type whose properties to check.</param>
        /// 
        /// <returns>Generated lambda function.</returns>
        /// 
        /// <exception cref="InvalidOperationException">There are no properties defined for the
        /// type.</exception>
        /// 
        /// <exception cref="NotSupportedException"><paramref name="type"/> is not an anonymous
        /// type. A common error is to use <see cref="EnsureNotNull"/> with an actual parameter,
        /// instead an object with property corresponding to the parameter. For example:
        ///     <code>
        ///         void MyMethod(object obj)
        ///         {
        ///             Arguments.EnsureNotNull(obj);   // Throws NotSupportedException
        /// 
        ///             // Instead, this should be
        ///             Arguments.EnsureNotNull( new {obj} );
        ///         }
        ///     </code>
        /// </exception>
        private static Action<object> CreateNullValidator(Type type)
        {
            // TODO: Is there a better way to detect anonymous type?
            // This code checks for presence of two symbols that can only be in the name of a compiler-generated type.
            if (!type.Name.Contains("<>"))
            { 
                throw new NotSupportedException(
                    string.Format("Error in argument to EnsureNotNull: you must wrap the argument in 'new {{arg}}' construct. Instead, you passed an argument of type {0}", type.Name)
                );
            }

            var properties = type.GetProperties();
            if (properties.Length == 0)
                throw new InvalidOperationException("No properties specified in call to EnsureNotNull");

            // Generate method that will check every property for null
            //
            //      AnonymousType inputs = (AnonymoustType)arg;
            //
            //      // An 'if' for every property
            //      if ( ReferenceEqual(inputs.Property1, null) )
            //          throw new ArgumentNullException("Property1");
            //      ...
            //      if ( ReferenceEqual(inputs.PropertyN, null) )
            //          throw new ArgumentNullException("PropertyN");
            var expInput = Expression.Parameter(typeof(object), "arg");

            // var inputs = (type)arg;
            var expObject = Expression.Variable(type, "inputs");
            var expAssign = Expression.Assign(expObject, Expression.ConvertChecked(expInput, type));

            // Create checks for every property
            var checks = properties.Select(property => CreateNullCheck(expObject, property));

            // We have to add the assignment operation as the first statement of the block
            var expResult = Expression.Block(new[]{expObject}, new[]{expAssign}.Concat(checks));
            return Expression.Lambda<Action<object>>(expResult, expInput).Compile();
        }

        /// <summary>
        /// Creates an expression that throws <see cref="ArgumentNullException"/> if the given
        /// property is <c>null</c>.
        /// </summary>
        /// 
        /// <param name="expObject">The expression corresponding to the object the property belongs
        /// to.</param>
        /// <param name="property">The property to check.</param>
        /// 
        /// <returns>Expression that checks the property and throws if it is <c>null</c>.</returns>
        private static Expression CreateNullCheck(Expression expObject, PropertyInfo property)
        {
            // if ( ReferenceEqual(expObject.Property, null) )
            //      throw new ArgumentNullException(property.Name);
            return Expression.IfThen(
                Expression.ReferenceEqual(Expression.Property(expObject, property), Expression.Constant(null)),
                Expression.Throw(Expression.New(_argumentNullExceptionConstructor, Expression.Constant(property.Name)))
            );
        }
    }
}