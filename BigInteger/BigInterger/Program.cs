using System.Diagnostics;
using System.Numerics;

namespace BigInterger
{
	internal class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine("==============================");
			BigIntegerUnit unit2 = new BigIntegerUnit("-500");
			BigIntegerUnit unit3 = new BigIntegerUnit(500);
			BigInteger cshparBigInteger1 = new BigInteger(-255255);
			BigInteger cshparBigInteger2 = new BigInteger(-255);
			Console.WriteLine(unit2.ToDexString());
			Console.WriteLine(unit3.ToDexString());
			Console.WriteLine("============덧셈==================");
			Console.WriteLine((unit2+unit3).ToDexString());
			Console.WriteLine((cshparBigInteger1+cshparBigInteger2).ToString());
			Console.WriteLine("============string==================");
			BigIntegerUnit unit5 = new BigIntegerUnit("-1234567981234567913245679");
			Console.WriteLine(unit5.ToDexString());
			BigInteger cshparBigInteger5 = BigInteger.Parse("-1234567981234567913245679");
			Console.WriteLine(cshparBigInteger5.ToString());
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
