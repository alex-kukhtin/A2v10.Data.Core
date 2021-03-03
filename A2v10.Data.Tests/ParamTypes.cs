// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace A2v10.Data.Tests
{
	public class TestParams
	{
		public String NVarChar { get; set; }
		public String VarChar { get; set; }
		public Byte[] ByteArray { get; set; }
		public Decimal Money { get; set; }
		public Double Real { get; set; }
		public DateTime Date { get; set; }
		public DateTime Time { get; set; }
		public DateTime DateTime { get; set; }
		public Boolean Boolean { get; set; }
	}

	public class TestParams2
	{
		public String NVarChar { get; set; }
		public String VarChar { get; set; }
		public Byte[] ByteArray { get; set; }
		public Decimal Money { get; set; }
		public Double Real { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan Time { get; set; }
		public DateTime DateTime { get; set; }
		public Boolean Boolean { get; set; }
	}

	[TestClass]
	[TestCategory("Parameter Types")]
	public class ParamTypes
	{
		readonly IDbContext _dbContext;
		public ParamTypes()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task Primitive()
		{
			var now = DateTime.Now;
			var bytes = new Byte[23];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (Byte) i;

			var prms = new TestParams()
			{
				NVarChar = "NVarChar",
				VarChar = "VarChar",
				Money = 123.34M,
				Real = 787.2345,
				Date = now,
				DateTime = now,
				Time = now,
				ByteArray = bytes,
				Boolean = true
			};

			var dm = await _dbContext.LoadModelAsync(null, "a2test.[ParamTypes.Load]", prms);

			var dat = new DataTester(dm, "Result");

			Assert.AreEqual("NVarChar", dat.GetValue<String>("NVarChar"));
			Assert.AreEqual("VarChar", dat.GetValue<String>("VarChar"));
			Assert.AreEqual(123.34M, dat.GetValue<Decimal>("Money"));
			Assert.AreEqual(787.2345D, dat.GetValue<Double>("Real"));

			var date = dat.GetValue<DateTime>("Date");
			Assert.IsTrue(date.Year == now.Year && date.Month == now.Month && date.Day == now.Day && 
				date.Hour ==  0  && date.Minute == 0 && date.Second == 0);
			date = dat.GetValue<DateTime>("DateTime");
			Assert.IsTrue(date.Year == now.Year && date.Month == now.Month && date.Day == now.Day &&
				date.Hour == now.Hour && date.Minute == now.Minute && Math.Abs(date.Second - now.Second) < 2);

			var ts = dat.GetValue<TimeSpan>("Time");
			Assert.IsTrue(ts.Hours == now.Hour && ts.Minutes == now.Minute && ts.Seconds == now.Second);

			var ba = dat.GetValue<Byte[]>("ByteArray");
			Assert.AreEqual(ba.Length, bytes.Length);
			for (int i = 0; i < ba.Length; i++)
				Assert.AreEqual(ba[i], bytes[i]);

			Assert.AreEqual(true, dat.GetValue<Boolean>("Boolean"));
		}

		[TestMethod]
		public async Task PrimitiveExecAndLoad()
		{
			var now = DateTime.Now;
			var bytes = new Byte[23];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (Byte)i;

			var prms = new TestParams2()
			{
				NVarChar = "NVarChar",
				VarChar = "VarChar",
				Money = 123.34M,
				Real = 787.2345,
				Date = now,
				DateTime = now,
				Time = TimeSpan.FromSeconds(1234),
				ByteArray = bytes,
				Boolean = true
			};

			var dm = await _dbContext.ExecuteAndLoadAsync<TestParams2, TestParams2>(null, "a2test.[ParamTypes.ExecLoad]", prms);


			Assert.AreEqual(prms.NVarChar, dm.NVarChar);
			Assert.AreEqual(prms.VarChar, dm.VarChar);
			Assert.AreEqual(prms.Money, dm.Money);
			Assert.AreEqual(prms.Real, dm.Real);
			Assert.AreEqual(prms.Time, dm.Time);

			var date = dm.Date;
			Assert.IsTrue(date.Year == now.Year && date.Month == now.Month && date.Day == now.Day &&
				date.Hour == 0 && date.Minute == 0 && date.Second == 0);
			date = dm.DateTime;
			Assert.IsTrue(date.Year == now.Year && date.Month == now.Month && date.Day == now.Day &&
				date.Hour == now.Hour && date.Minute == now.Minute && Math.Abs(date.Second - now.Second) < 2);


			var ba = dm.ByteArray;
			Assert.AreEqual(ba.Length, bytes.Length);
			for (int i = 0; i < ba.Length; i++)
				Assert.AreEqual(ba[i], bytes[i]);

			Assert.AreEqual(prms.Boolean, dm.Boolean);
		}

		[TestMethod]
		public async Task NullableStringSaveModel()
		{

			var eo = new ExpandoObject();
			var doc = new ExpandoObject();
			doc.Set("Name", "");
			eo.Set("Document", doc);


			var dm = await _dbContext.SaveModelAsync(null, "a2test.[Nullable.SaveModel.Update]", eo);


			var dt = new DataTester(dm, "Document");
			Assert.AreEqual(true, dt.GetValue<Boolean>("NameIsNull"));


			int z = 55;
		}

	}

}
