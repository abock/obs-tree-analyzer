// 
// Engine.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TextTemplating;

namespace Mono.TextTemplating
{
	
	public class TemplatingEngine : MarshalByRefObject, Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngine
	{
		
		public string ProcessTemplate (string content, ITextTemplatingEngineHost host)
		{
			var tpl = CompileTemplate (content, host);
			if (tpl != null)
				return tpl.Process ();
			return null;
		}

		public CompiledTemplate CompileTemplate (string content, ITextTemplatingEngineHost host)
		{
			ParsedTemplate pt = ParsedTemplate.FromText (content, host);
			if (pt.Errors.HasErrors) {
				host.LogErrors (pt.Errors);
				return null;
			}
			
			TemplateSettings settings = GetSettings (host, pt);
			if (pt.Errors.HasErrors) {
				host.LogErrors (pt.Errors);
				return null;
			}
			
			CodeCompileUnit ccu = GenerateCompileUnit (host, pt, settings);
			if (pt.Errors.HasErrors) {
				host.LogErrors (pt.Errors);
				return null;
			}
			
			System.Reflection.Assembly results = GenerateCode (host, pt, settings, ccu);
			if (pt.Errors.HasErrors) {
				host.LogErrors (pt.Errors);
				return null;
			}
			
			if (!String.IsNullOrEmpty (settings.Extension)) {
				host.SetFileExtension (settings.Extension);
			}
			if (settings.Encoding != null) {
				//FIXME: when is this called with false?
				host.SetOutputEncoding (settings.Encoding, true);
			}
			
			return new CompiledTemplate (pt, host, results, settings);
		}
		
		public static System.Reflection.Assembly GenerateCode (ITextTemplatingEngineHost host, ParsedTemplate pt, 
		                                                       TemplateSettings settings, CodeCompileUnit ccu)
		{
			CompilerParameters pars = new CompilerParameters ();
			pars.GenerateExecutable = false;
			
			if (settings.Debug) {
				pars.GenerateInMemory = false;
				pars.IncludeDebugInformation = true;
				pars.TempFiles.KeepFiles = true;
			} else {
				pars.GenerateInMemory = true;
				pars.IncludeDebugInformation = false;
			}

			//resolve and add assembly references
			HashSet<string> assemblies = new HashSet<string> ();
			assemblies.UnionWith (settings.Assemblies);
			assemblies.UnionWith (host.StandardAssemblyReferences);
			foreach (string assem in assemblies) {
				string resolvedAssem = host.ResolveAssemblyReference (assem);
				if (!String.IsNullOrEmpty (resolvedAssem)) {
					pars.ReferencedAssemblies.Add (resolvedAssem);
				} else {
					pt.LogError ("Could not resolve assembly reference '" + assem + "'");
					return null;
				}
			}
			CompilerResults results = settings.Provider.CompileAssemblyFromDom (pars, ccu);
			pt.Errors.AddRange (results.Errors);
			if (pt.Errors.HasErrors)
				return null;
			return results.CompiledAssembly;
		}
		
		public static TemplateSettings GetSettings (ITextTemplatingEngineHost host, ParsedTemplate pt)
		{
			string language = null;
			
			TemplateSettings settings = new TemplateSettings ();
			foreach (Directive dt in pt.Directives) {
				switch (dt.Name) {
				case "template":
					string val = dt.Extract ("language");
					if (val != null)
						language = val;
					val = dt.Extract ("debug");
					if (val != null)
						settings.Debug = string.Compare (val, "true", StringComparison.OrdinalIgnoreCase) == 0;
					val = dt.Extract ("inherits");
					if (val != null)
						settings.Inherits = val;
					val = dt.Extract ("culture");
					if (val != null) {
						System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.GetCultureInfo (val);
						if (culture == null)
							pt.LogWarning ("Could not find culture '" + val + "'", dt.StartLocation);
						else
							settings.Culture = culture;
					}
					val = dt.Extract ("hostspecific");
					if (val != null) {
						settings.HostSpecific = string.Compare (val, "true", StringComparison.OrdinalIgnoreCase) == 0;
					}
					break;
					
				case "assembly":
					string name = dt.Extract ("name");
					if (name == null)
						pt.LogError ("Missing name attribute in assembly directive", dt.StartLocation);
					else
						settings.Assemblies.Add (name);
					break;
					
				case "import":
					string namespac = dt.Extract ("namespace");
					if (namespac == null)
						pt.LogError ("Missing namespace attribute in import directive", dt.StartLocation);
					else
						settings.Imports.Add (namespac);
					break;
					
				case "output":
					settings.Extension = dt.Extract ("extension");
					string encoding = dt.Extract ("encoding");
					if (encoding != null)
						settings.Encoding = Encoding.GetEncoding ("encoding");
					break;
				
				case "include":
					throw new InvalidOperationException ("Include is handled in the parser");
					
				default:
					throw new NotImplementedException ("Custom directives are not supported yet");
				}
				ComplainExcessAttributes (dt, pt);
			}
			
			if (settings.Name == null)
				settings.Name = string.Format ("GeneratedTextTransformation{0:x}", new System.Random ().Next ());
			if (settings.Namespace == null)
				settings.Namespace = typeof (TextTransformation).Namespace;
			
			//resolve the CodeDOM provider
			if (String.IsNullOrEmpty (language)) {
				pt.LogError ("No language was specified for the template");
				return settings;
			}
			
			if (language == "C#v3.5") {
				Dictionary<string, string> providerOptions = new Dictionary<string, string> ();
				providerOptions.Add ("CompilerVersion", "v3.5");
				settings.Provider = new CSharpCodeProvider (providerOptions);
			}
			else {
				settings.Provider = CodeDomProvider.CreateProvider (language);
			}
			
			if (settings.Provider == null) {
				pt.LogError ("A provider could not be found for the language '" + language + "'");
				return settings;
			}
			
			return settings;
		}
		
		static bool ComplainExcessAttributes (Directive dt, ParsedTemplate pt)
		{
			if (dt.Attributes.Count == 0)
				return false;
			StringBuilder sb = new StringBuilder ("Unknown attributes ");
			bool first = true;
			foreach (string key in dt.Attributes.Keys) {
				if (!first) {
					sb.Append (", ");
				} else {
					first = false;
				}
				sb.Append (key);
			}
			sb.Append (" found in ");
			sb.Append (dt.Name);
			sb.Append (" directive.");
			pt.LogWarning (sb.ToString (), dt.StartLocation);
			return false;
		}
		
		public static CodeCompileUnit GenerateCompileUnit (ITextTemplatingEngineHost host, ParsedTemplate pt,
		                                                   TemplateSettings settings)
		{
			//prep the compile unit
			var ccu = new CodeCompileUnit ();
			var namespac = new CodeNamespace (settings.Namespace);
			ccu.Namespaces.Add (namespac);
			
			var imports = new HashSet<string> ();
			imports.UnionWith (settings.Imports);
			imports.UnionWith (host.StandardImports);
			foreach (string ns in imports)
				namespac.Imports.Add (new CodeNamespaceImport (ns));
			
			//prep the type
			var type = new CodeTypeDeclaration (settings.Name);
			if (!String.IsNullOrEmpty (settings.Inherits))
				type.BaseTypes.Add (new CodeTypeReference (settings.Inherits));
			else
				type.BaseTypes.Add (new CodeTypeReference (typeof (TextTransformation)));
			namespac.Types.Add (type);
			
			//prep the transform method
			var transformMeth = new CodeMemberMethod () {
				Name = "TransformText",
				ReturnType = new CodeTypeReference (typeof (String)),
				Attributes = MemberAttributes.Public | MemberAttributes.Override
			};
			
			//method references that will need to be used multiple times
			var writeMeth = new CodeMethodReferenceExpression (new CodeThisReferenceExpression (), "Write");
			var toStringMeth = new CodeMethodReferenceExpression (new CodeTypeReferenceExpression (typeof (ToStringHelper)), "ToStringWithCulture");
			bool helperMode = false;
			
			//build the code from the segments
			foreach (TemplateSegment seg in pt.Content) {
				CodeStatement st = null;
				var location = new CodeLinePragma (seg.StartLocation.FileName ?? host.TemplateFile, seg.StartLocation.Line);
				switch (seg.Type) {
				case SegmentType.Block:
					if (helperMode)
						//TODO: are blocks permitted after helpers?
						throw new ParserException ("Blocks are not permitted after helpers", seg.StartLocation);
					st = new CodeSnippetStatement (seg.Text);
					break;
				case SegmentType.Expression:
					st = new CodeExpressionStatement (
						new CodeMethodInvokeExpression (writeMeth,
							new CodeMethodInvokeExpression (toStringMeth, new CodeSnippetExpression (seg.Text))));
					break;
				case SegmentType.Content:
					st = new CodeExpressionStatement (new CodeMethodInvokeExpression (writeMeth, new CodePrimitiveExpression (seg.Text)));
					break;
				case SegmentType.Helper:
					type.Members.Add (new CodeSnippetTypeMember (seg.Text) { LinePragma = location });
					helperMode = true;
					break;
				default:
					throw new InvalidOperationException ();
				}
				if (st != null) {
					if (helperMode) {
						//convert the statement into a snippet member and attach it to the top level type
						//TODO: is there a way to do this for languages that use indentation for blocks, e.g. python?
						using (var writer = new StringWriter ()) {
							settings.Provider.GenerateCodeFromStatement (st, writer, null);
							type.Members.Add (new CodeSnippetTypeMember (writer.ToString ()) { LinePragma = location });
						}
					} else {
						st.LinePragma = location;
						transformMeth.Statements.Add (st);
						continue;
					}
				}
			}
			
			//complete the transform method
			transformMeth.Statements.Add (new CodeMethodReturnStatement (
				new CodeMethodInvokeExpression (
					new CodePropertyReferenceExpression (
						new CodeThisReferenceExpression (),
						"GenerationEnvironment"),
						"ToString")));
			type.Members.Add (transformMeth);
			
			//generate the Host property if needed
			if (settings.HostSpecific) {
				var hostField = new CodeMemberField (new CodeTypeReference (typeof (ITextTemplatingEngineHost)), "hostValue");
				hostField.Attributes = (hostField.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Private;
				type.Members.Add (hostField);
				
				var hostProp = new CodeMemberProperty () {
					Name = "Host",
					Attributes = MemberAttributes.Public,
					HasGet = true,
					HasSet = true,
					Type = hostField.Type
				};
				var hostFieldRef = new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), "hostValue");
				hostProp.SetStatements.Add (new CodeAssignStatement (hostFieldRef, new CodePropertySetValueReferenceExpression ()));
				hostProp.GetStatements.Add (new CodeMethodReturnStatement (hostFieldRef));
				type.Members.Add (hostProp);
			}
			
			return ccu;
		}
		
		public static string Run (System.Reflection.Assembly assem, string type, ITextTemplatingEngineHost host, System.Globalization.CultureInfo culture)
		{
			Type transformType = assem.GetType (type);
			TextTransformation tt;

			IExtendedTextTemplatingEngineHost extendedHost = host as IExtendedTextTemplatingEngineHost;
			if (extendedHost != null) {
				tt = extendedHost.CreateInstance (transformType);
			}
			else {
				tt = (TextTransformation) Activator.CreateInstance (transformType);
			}
			
			//set the host property if it exists
			System.Reflection.PropertyInfo hostProp = transformType.GetProperty ("Host", typeof (ITextTemplatingEngineHost));
			if (hostProp != null && hostProp.CanWrite)
				hostProp.SetValue (tt, host, null);
			
			//set the culture
			if (culture != null)
				ToStringHelper.FormatProvider = culture;
			else
				ToStringHelper.FormatProvider = System.Globalization.CultureInfo.InvariantCulture;
			
			tt.Initialize ();
			string output = tt.TransformText ();
			host.LogErrors (tt.Errors);
			ToStringHelper.FormatProvider = System.Globalization.CultureInfo.InvariantCulture;
			return output;
		}
	}
}
