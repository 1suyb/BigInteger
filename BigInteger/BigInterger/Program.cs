using System.Diagnostics;
using System.Numerics;

namespace BigInterger
{
	internal class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine("==============================");
			BigIntegerUnit unit2 = new BigIntegerUnit("-1231321465431874968546847698476847683547684786476897687987654");
			BigIntegerUnit unit3 = new BigIntegerUnit(255);
			BigInteger cshparBigInteger1 = new BigInteger(255255255255);
			BigInteger cshparBigInteger2 = new BigInteger(255);
			Console.WriteLine(unit2.ToString());
			Console.WriteLine(unit3.ToString());
			Console.WriteLine(cshparBigInteger1.ToString("X"));
			Console.WriteLine(cshparBigInteger2.ToString("X"));
			Console.WriteLine("============나눗셈==================");
			Console.WriteLine((unit2/unit3).ToHexString());
			Console.WriteLine((cshparBigInteger1/cshparBigInteger2).ToString("X"));
			Console.WriteLine("============string==================");
			BigIntegerUnit unit5 = new BigIntegerUnit("-1234567981234567913245679");
			Console.WriteLine(unit5.ToDexString());
			BigInteger cshparBigInteger5 = BigInteger.Parse("-1234567981234567913245679");
			Console.WriteLine(cshparBigInteger5.ToString("X"));
			Console.WriteLine("==============대소비교================");
			Console.WriteLine((unit2>unit3));
			Console.WriteLine((unit2<unit3));
			Console.WriteLine((unit2>=unit3));
			Console.WriteLine((unit2<=unit3));
			Console.WriteLine((unit2==unit3));
			Console.WriteLine((unit2!=unit3));
			Console.WriteLine("==============================");
			Console.WriteLine(BigIntegerUnit.ToInt64(unit2));
			Console.WriteLine(unit2.ToDexString());
		}
	}
}
