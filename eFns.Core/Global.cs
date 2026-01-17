/**
 * @Name Global.cs
 * @Purpose 
 * @Date 11 February 2025, 09:50:00
 * @Author S.Deckers
 * @Description 
 */

namespace eFns.Core
{
	#region -- Using directives --
	using System.Diagnostics;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Text.Json;
	using System.Text.Json.Serialization;

	using d=System.Diagnostics.Debug;
	#endregion
	
	public partial class Global
	{ 
		public static int CallCount = 0;

		[ Conditional("DEBUG")]
		public static void WriteLine(string theType, [CallerMemberName]string methodName = "")
		{
			d.WriteLine( $"{theType}.{methodName} ({Environment.CurrentManagedThreadId}.{Global.CallCount++})");
		}

		[Conditional( "DEBUG" )]
		public static void WriteLine( string theType, string info, [CallerMemberName] string methodName = "" )
		{
			d.WriteLine( $"{theType}.{methodName} ({Environment.CurrentManagedThreadId}.{Global.CallCount++}) {info}" );
		}
	}

	public partial class Global
	{
		/// <examples>
		/// 
		/// Common.Global.SerializeJson<RequestedSampleContainers>( sampleContainer, @$"c:\Usr\Stephan\Junk\{sampleContainer.Code}.json");
		/// Common.Global.SerializeJson<LabContainersWithAnalyticalDetails>( labContainersWithNextActivityDueDate, @$"c:\Usr\Stephan\Junk\la.{container}.json");
		/// 
		/// </example>
		/// 
		/// eFns.SampleReceptionWater\SampleReceptionWater\SampleReceptionWater.Service\SamplesService.cs :
		///	184  	Common.Global.SerializeJson<RequestedSampleContainers>( sampleContainer, @$"c:\Usr\Stephan\Junk\{sampleContainer.Code}.json");
		///	185
		/// 186     var parcelBarcode = sampleContainer.SampleContainer.Parcel.TrackingNumber ?? "-";
		/// 
		/// LabRules
		/// 
		/// eFns.Core.Global.SerializeJson<InputInfo>( inputInfo, @"c:\Usr\Stephan\Junk\labRules.json");
		public static void SerializeJson<T>( T t, string file)
		{
			if( System.IO.File.Exists( file))
			{
				System.IO.File.Delete( file);
			}

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNamingPolicy	= JsonNamingPolicy.CamelCase
			,	WriteIndented			= true
			,	DefaultIgnoreCondition	= JsonIgnoreCondition.WhenWritingNull
			};

			string json = System.Text.Json.JsonSerializer.Serialize<T>(t, options);

			using( System.IO.StreamWriter stream = new StreamWriter(file))
			{
				stream.Write(json);
				stream.Close();
			}
		}

		public static T DeSerializeJson<T>( string file)
		{
			if( !System.IO.File.Exists( file))
			{
				throw new Exception( $"Can't open file [{file}]");
			}

			string json = File.ReadAllText(file);

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNamingPolicy	= JsonNamingPolicy.CamelCase
			,	WriteIndented			= true
			};

			T item = System.Text.Json.JsonSerializer.Deserialize<T>(json, options)!;
			return( item);
		}

	}
}
