using System;
using System.Collections;
using System.Text;



public class BigIntegerUnit
{
	private byte[] _value;

	public byte[] Value
	{
		get => _value;
	}

	public int Count
	{
		get => _value.Length;
	}

	public bool IsNegative => (_value[Count - 1] & MostSignificantBit) == MostSignificantBit;
	public bool IsZero
	{
		get
		{
			for (int i = 0; i < Count; i++)
			{
				if (Value[i] != 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	public static readonly byte FullBit;
	public static readonly byte MostSignificantBit;
	public int BitLength => _value.Length * 8;
	public const int BytePerBit = 8;

	static BigIntegerUnit()
	{
		unchecked
		{
			FullBit = (byte)~((byte)0);
			MostSignificantBit = (byte)(1 << 7);
		}
	}

	public BigIntegerUnit(byte[] array)
	{
		_value = new byte[array.Length];
		Array.Copy(array, 0, _value, 0, _value.Length);

	}

	public BigIntegerUnit(List<byte> array)
	{
		_value = array.ToArray();
	}

	public BigIntegerUnit(BigIntegerUnit value)
	{
		_value = new byte[value.Count];
		Array.Copy(value.Value, 0, _value, 0, _value.Length);
	}

	public BigIntegerUnit(int value)
	{
		bool isNagtive = value < 0;
		if (isNagtive)
		{
			value = -value;
		}
		int size = sizeof(int);
		_value = new byte[size];
		for (int i = 0; i < size; i++)
		{
			_value[i] = (byte)(value >> BytePerBit * i);
		}
		if (isNagtive) _value = ToTwosComplement(_value).Value;
	}

	public BigIntegerUnit(uint value)
	{
		int size = sizeof(uint);
		_value = new byte[size];
		for (int i = 0; i < size; i++)
		{
			_value[i] = (byte)(value >> BytePerBit * i);
		}
	}

	public BigIntegerUnit(ulong value)
	{
		int size = sizeof(ulong);
		_value = new byte[size];
		for (int i = 0; i < size; i++)
		{
			_value[i] = (byte)(value >> BytePerBit * i);
		}
	}

	public BigIntegerUnit(string value)
	{
		// 012 345 678 91011 121314 151617 181920 21
		// "123,456,789,123,456,789,123"
		// 91011 121314 151617
		// "456,789,123" "456,789,132" "123"
		bool isNegative = value[0] == '-';
		value = value.Replace(",", "");
		value = value.Replace(" ", "");
		value = value.Replace("_", "");
		value = value.Replace("-", "");

		int valueLength = value.Length;
		BigIntegerUnit multiple = new BigIntegerUnit(1);
		BigIntegerUnit unit = new BigIntegerUnit(0);
		for (int i = value.Length - 1; i >= 0; i--)
		{
			int num = int.Parse(value.Substring(i, 1), System.Globalization.NumberStyles.HexNumber);
			unit += num * multiple;
			multiple *= 10;
		}

		byte[] positiveArray = TrimZeros(unit.Value);
		if (isNegative)
		{
			BigIntegerUnit negativeUnit = ToTwosComplement(positiveArray);
			_value = new byte[negativeUnit.Count];
			Array.Copy(negativeUnit.Value, 0, _value, 0, negativeUnit.Count);
		}

		else
		{
			_value = positiveArray;
		}
	}


	private static byte[] TrimZeros(byte[] onlyPositiveArray)
	{
		int length = onlyPositiveArray.Length;
		int noZeroIndex = 0;
		for (int i = length - 1; i > 0; i--)
		{
			if (onlyPositiveArray[i] != 0)
			{
				noZeroIndex = i;
				break;
			}
		}

		byte[] result = new byte[noZeroIndex + 2];
		Array.Copy(onlyPositiveArray, 0, result, 0, noZeroIndex + 1);
		return result;
	}

	public static BigIntegerUnit ToTwosComplement(BigIntegerUnit value)
	{
		byte[] result = new byte[value.Count];
		for (int i = 0; i < result.Length; i++)
		{
			result[i] = (byte)(~value.Value[i]);
		}

		int sum = 1;
		for (int i = 0; i < result.Length; i++)
		{
			sum += result[i];
			result[i] = (byte)(sum);
			sum = sum >> BytePerBit;
		}
		return new BigIntegerUnit(result);
	}
	public static BigIntegerUnit ToTwosComplement(byte[] value)
	{
		return ToTwosComplement(new BigIntegerUnit(value));
	}

	public static BigIntegerUnit operator +(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		int length = (int)MathF.Max(leftSide.Count, rightSide.Count);
		List<byte> result = new List<byte>();
		int carry = 0;

		for (int i = 0; i < length; i++)
		{
			byte bitLeft = (i < leftSide.Count) ? leftSide.Value[i] : (byte)0;
			byte bitRight = (i < rightSide.Count) ? rightSide.Value[i] : (byte)0;
			int sum = (bitLeft + bitRight + carry);
			result.Add((byte)(sum & FullBit));
			carry = (sum >> BytePerBit);
		}

		if (carry != 0)
		{
			if (!(leftSide.IsNegative || rightSide.IsNegative))
			{
				result.Add((byte)carry);
			}
		}
		return new BigIntegerUnit(result);
	}
	public static BigIntegerUnit operator +(BigIntegerUnit leftSide, int rightSide)
	{
		BigIntegerUnit rightSideUnit = new BigIntegerUnit(rightSide);
		return leftSide + rightSideUnit;
	}

	public static BigIntegerUnit operator -(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit right = ToTwosComplement(rightSide);
		return leftSide + right;
	}

	public static BigIntegerUnit operator -(BigIntegerUnit leftSide)
	{
		return ToTwosComplement(leftSide);
	}

	public static BigIntegerUnit operator *(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		bool isNegative = leftSide.IsNegative != rightSide.IsNegative;
		byte[] result = new byte[leftSide.Count + rightSide.Count];
		for (int i = 0; i < leftSide.Count; i++)
		{
			int carry = 0;
			for (int j = 0; j < rightSide.Count; j++)
			{

				int multiple = (leftSide.Value[i] * rightSide.Value[j]) + (int)result[i + j];
				result[i + j] = (byte)(multiple & FullBit);
				carry = (multiple >> BytePerBit);
				result[i + j + 1] = (byte)carry;
			}
		}
		result = TrimZeros(result);
		return isNegative ? -new BigIntegerUnit(result) : new BigIntegerUnit(result);
	}
	public static BigIntegerUnit operator *(BigIntegerUnit leftSide, int rightSide)
	{
		BigIntegerUnit rightSideUnit = new BigIntegerUnit(rightSide);
		return leftSide * rightSideUnit;
	}
	public static BigIntegerUnit operator *(int leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit leftSideUnit = new BigIntegerUnit(leftSide);
		return leftSideUnit * rightSide;
	}



	public static bool operator >(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit result = leftSide-rightSide;
		if (result.IsNegative)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	public static bool operator <(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit result = leftSide - rightSide;
		if (result.IsNegative)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public static bool operator >=(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		return !(leftSide < rightSide);
	}
	public static bool operator <=(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		return !(rightSide > leftSide);
	}


	private static byte[] CopyArrayFrom(BigIntegerUnit unit)
	{
		byte[] result = new byte[unit.Count];
		Array.Copy(unit.Value, 0, result, 0, unit.Count);
		return result;
	}


	public static BigIntegerUnit Add(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		return leftSide + rightSide;
	}

	public static BigIntegerUnit Add(BigIntegerUnit leftSide, int rightSide)
	{
		return leftSide + new BigIntegerUnit(rightSide);
	}

	public static BigIntegerUnit operator <<(BigIntegerUnit bigIntegerUnit, int shiftValue)
	{
		return LeftShift(bigIntegerUnit.Value, shiftValue);
	}
	public static BigIntegerUnit operator >>(BigIntegerUnit bigIntegerUnit, int shiftValue)
	{
		return RightShift(bigIntegerUnit.Value, shiftValue);
	}
	public static BigIntegerUnit LeftShift(byte[] byteArray, int shiftAmount)
	{
		int bitLength = byteArray.Length * 8;

		if (shiftAmount == 0)
		{
			return new BigIntegerUnit(byteArray);
		}

		int newBitLength = bitLength + shiftAmount;
		int newByteLength = (newBitLength + 7) / 8;
		byte[] shiftedBytes = new byte[newByteLength];

		int byteShift = shiftAmount / 8;
		int bitShift = shiftAmount % 8;

		for (int i = 0; i < byteArray.Length; i++)
		{
			int shiftedIndex = i + byteShift;

			if (shiftedIndex < newByteLength)
			{
				// 현재 바이트를 bitShift만큼 왼쪽으로 이동시키고 결과를 shiftedBytes[shiftedIndex]에 저장
				shiftedBytes[shiftedIndex] |= (byte)(byteArray[i] << bitShift);
			}

			// 다음 바이트로 넘기는 비트 추가
			if (shiftedIndex + 1 < newByteLength && bitShift > 0)
			{
				shiftedBytes[shiftedIndex + 1] |= (byte)(byteArray[i] >> (8 - bitShift));
			}
		}

		return new BigIntegerUnit(shiftedBytes);
	}
	public static BigIntegerUnit RightShift(byte[] byteArray, int shiftAmount)
	{
		int bitLength = byteArray.Length * 8;
		if (shiftAmount == 0)
		{
			return new BigIntegerUnit(byteArray);
		}
		int newBitLength = bitLength - shiftAmount;
		int newByteLength = (newBitLength + 7) / 8;
		byte[] shiftedBytes = new byte[newByteLength];

		int byteShift = shiftAmount / 8;
		int bitShift = shiftAmount % 8;

		for (int i = byteArray.Length - 1; i >= 0; i--)
		{
			int shiftedIndex = i - byteShift;
			if (shiftedIndex >= 0)
			{
				shiftedBytes[shiftedIndex] |= (byte)(byteArray[i] >> bitShift);
			}

			if (shiftedIndex - 1 >= 0 && bitShift > 0)
			{
				shiftedBytes[shiftedIndex - 1] |= (byte)(byteArray[i] << (8 - bitShift));
			}
		}

		return new BigIntegerUnit(shiftedBytes);
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < Count; i++)
		{
			sb.Append($"{_value[i]} ");
		}
		return sb.ToString();
	}

}


public class BigInteger
{

}
