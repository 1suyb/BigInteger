using System.Diagnostics;

namespace BigInterger
{
	internal class Program
	{
		static void Main(string[] args)
		{
			byte[] bytes = new byte[] { 0b11111111, 0b11110000, 0b00000000 };
			foreach (var item in bytes)
			{
				string binaryString = Convert.ToString(item, 2).PadLeft(8, '0'); // 2진수 문자열로 변환 (8비트 패딩)
				Console.WriteLine(binaryString);
			}
			Console.WriteLine("==============================");
			Console.WriteLine(sizeof(int));
			BigIntegerUnit unit2 = new BigIntegerUnit(2000);
			Console.WriteLine(unit2.ToString());
			Console.WriteLine(BigIntegerUnit.ToTwosComplement(unit2).ToString());
			BigIntegerUnit unit3 = new BigIntegerUnit(4000);
			Console.WriteLine(unit3.ToString());
			Console.WriteLine("==============================");

			BigIntegerUnit unit4 = new BigIntegerUnit("1");
			Console.WriteLine(unit4.ToString());
			BigIntegerUnit unit5 = new BigIntegerUnit("-01");
			Console.WriteLine(unit5.ToString());

			Console.WriteLine((unit2 * unit3).ToString());
		}
	}
}
