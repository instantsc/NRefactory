﻿// 
// RemoveRegion.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc. (http://xamarin.com)
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
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp;

namespace ICSharpCode.NRefactory6.CSharp.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name="Remove #region/#endregion directives")]
	public class RemoveRegionEndRegionDirectivesCodeRefactoringProvider : CodeRefactoringProvider
	{
		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var document = context.Document;
			if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
				return;
			var span = context.Span;
			if (!span.IsEmpty)
				return;
			var cancellationToken = context.CancellationToken;
			if (cancellationToken.IsCancellationRequested)
				return;
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (model.IsFromGeneratedCode())
				return;
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

			SyntaxTrivia directive;
			if (!TryGetDirective(root, span, out directive))
				return;

			context.RegisterRefactoring(
				CodeActionFactory.Create(
					directive.Span,
					DiagnosticSeverity.Info,
					GettextCatalog.GetString ("Remove region/endregion directives"),
					t2 => {
						var structure = directive.GetStructure();
						// var prev = directive.GetPreviousTrivia (model.SyntaxTree, cancellationToken, true);
						var end = structure as DirectiveTriviaSyntax;
						SourceText text = document.GetTextAsync (cancellationToken).Result;
						foreach (var e in end.GetRelatedDirectives().OrderByDescending (e => e.SpanStart)){
							var line = text.Lines.GetLineFromPosition (e.FullSpan.Start);
							text = text.Replace (line.SpanIncludingLineBreak, "");
						}
						return Task.FromResult(document.WithText (text));
					}
				)
			);
		}

		static bool TryGetDirective (SyntaxNode root, TextSpan span, out SyntaxTrivia directive)
		{
			directive = root.FindTrivia(span.Start);
			return directive.IsKind(SyntaxKind.RegionDirectiveTrivia) || directive.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
		}
	}
}