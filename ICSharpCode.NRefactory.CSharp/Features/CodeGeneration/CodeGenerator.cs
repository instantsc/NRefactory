﻿//
// CodeGenerator.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ICSharpCode.NRefactory6.CSharp.CodeGeneration;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace ICSharpCode.NRefactory6.CSharp
{
	public static class CodeGenerator
	{
		readonly static Type typeInfo;

		static CodeGenerator ()
		{
			typeInfo = Type.GetType ("Microsoft.CodeAnalysis.CodeGeneration.CodeGenerator" + ReflectionNamespaces.WorkspacesAsmName, true);
			addPropertyDeclarationAsyncMethod = typeInfo.GetMethod ("AddPropertyDeclarationAsync", BindingFlags.Static | BindingFlags.Public);
			addMethodDeclarationAsyncMethod = typeInfo.GetMethod ("AddMethodDeclarationAsync", BindingFlags.Static | BindingFlags.Public);
			addFieldDeclarationAsyncMethod = typeInfo.GetMethod ("AddFieldDeclarationAsync", BindingFlags.Static | BindingFlags.Public);
		}

		readonly static MethodInfo addFieldDeclarationAsyncMethod;

		public static Task<Document> AddFieldDeclarationAsync(Solution solution, INamedTypeSymbol destination, IFieldSymbol field, CodeGenerationOptions options = default(CodeGenerationOptions), CancellationToken cancellationToken = default(CancellationToken))
		{
			return (Task<Document>)addPropertyDeclarationAsyncMethod.Invoke (null, new object[] { solution, destination, field, options != null ? options.Instance : null, cancellationToken });
		}


		readonly static MethodInfo addPropertyDeclarationAsyncMethod;
		public static Task<Document> AddPropertyDeclarationAsync(Solution solution, INamedTypeSymbol destination, IPropertySymbol property, CodeGenerationOptions options = default(CodeGenerationOptions), CancellationToken cancellationToken = default(CancellationToken))
		{
			return (Task<Document>)addPropertyDeclarationAsyncMethod.Invoke (null, new object[] { solution, destination, property, options != null ? options.Instance : null, cancellationToken });
		}

		readonly static MethodInfo addMethodDeclarationAsyncMethod;
		public static Task<Document> AddMethodDeclarationAsync(Solution solution, INamedTypeSymbol destination, IMethodSymbol method, CodeGenerationOptions options = default(CodeGenerationOptions), CancellationToken cancellationToken = default(CancellationToken))
		{
			return (Task<Document>)addMethodDeclarationAsyncMethod.Invoke (null, new object[] { solution, destination, method, options != null ? options.Instance : null, cancellationToken });
		}

		public static Task<Document> AddMemberDeclarationsAsync(Solution solution, INamedTypeSymbol destination, IEnumerable<ISymbol> members, CodeGenerationOptions options = default(CodeGenerationOptions), CancellationToken cancellationToken = default(CancellationToken))
		{
			return  new CSharpCodeGenerationService(solution.Workspace, destination.Language).AddMembersAsync(solution, destination, members, options, cancellationToken);
		}

		public static bool CanAdd(Solution solution, ISymbol destination, CancellationToken cancellationToken = default(CancellationToken))
		{
			return new CSharpCodeGenerationService(solution.Workspace, destination.Language).CanAddTo(destination, solution, cancellationToken);
		}
	}
}

