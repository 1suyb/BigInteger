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
	public int BitLength => _value.Length* 8;
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
		_value = TrimZeros(_value);
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

		_value = TrimZeros(_value);
	}

	public BigIntegerUnit(ulong value)
	{
		int size = sizeof(ulong);
		_value = new byte[size];
		for (int i = 0; i < size; i++)
		{
			_value[i] = (byte)(value >> BytePerBit * i);
		}

		_value = TrimZeros(_value);
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
			BigIntegerUnit sum = num * multiple;
			unit += sum;
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

	private static int GetHighestBitPosition(byte[] value)
	{
		int noZeroIndex = 0;
		for (int i = value.Length-1 ; i >=0 ; i--)
		{
			if(value[i] != 0)
			{
				noZeroIndex = i;
				break;
			}
		}

		int position = 0;
		byte b = value[noZeroIndex];
		while (b > 0)
		{
			b >>= 1;
			position++;
		}

		position += (noZeroIndex + 1) * BigIntegerUnit.BytePerBit;
		return position;
	}
	private static byte[] TrimZeros(byte[] onlyPositiveArray)
	{
		int length = onlyPositiveArray.Length;
		int noZeroIndex = 0;
		bool isfullbit = false;
		for (int i = length - 1; i > 0; i--)
		{
			if (onlyPositiveArray[i] != 0)
			{
				noZeroIndex = i;
				if (onlyPositiveArray[i] == 255)
				{
					isfullbit = true;
				}
				break;
			}
		}
		
		byte[] result = new byte[noZeroIndex+2];
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
		leftSide = Abs(leftSide);
		rightSide= Abs(rightSide);
		
		byte[] result = new byte[leftSide.Count + rightSide.Count];
		int carry = 0;
		for (int i = 0; i < leftSide.Count; i++)
		{
			int k = i;
			for (int j = 0; j < rightSide.Count; j++)
			{
				k = i + j;
				int multiple = (leftSide.Value[i] * rightSide.Value[j]) + carry + result[i+j];
				result[i+j] = (byte)(multiple);
				carry = (multiple >> BytePerBit);
			}
			while (carry>0)
			{
				k++;
				result[k] = (byte)(carry);
				carry = (carry >> BytePerBit);
			}
			
		}
		byte[] output = TrimZeros(result);
		return isNegative ? -new BigIntegerUnit(output) : new BigIntegerUnit(output);
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

	public static BigIntegerUnit operator /(BigIntegerUnit dividend, BigIntegerUnit divisor)
	{
		if (divisor.IsZero)
		{
			throw new DivideByZeroException("Cannot divide by zero.");
		}
		if (dividend.IsZero)
		{
			return new BigIntegerUnit(0);
		}
		
		bool isNegativeResult = dividend.IsNegative != divisor.IsNegative;

		BigIntegerUnit absDividend = dividend.IsNegative ? -dividend : dividend;
		BigIntegerUnit absDivisor = divisor.IsNegative ? -divisor : divisor;

		if (absDivisor > absDividend)
		{
			return new BigIntegerUnit(0);
		}

		BigIntegerUnit quotient = new BigIntegerUnit(0);
		BigIntegerUnit remainder = new BigIntegerUnit(absDividend);

		// 이 라인은 나누어지는 값과 나누는 값의 비트 길이 차이를 계산하여, 처음에 나누는 값을 얼마나 왼쪽으로 이동시킬지 결정합니다.
		int dividendHighestBit = GetHighestBitPosition(absDividend.Value); // 가장 마지막 바이트에서 가장 높은 비트 위치를 구함
		int divisorHighestBit = GetHighestBitPosition(absDivisor.Value); // 가장 마지막 바이트에서 가장 높은 비트 위치를 구함

		int bitLengthDifference = dividendHighestBit - divisorHighestBit;


		// 이 라인은 나누는 값을 비트 길이 차이만큼 왼쪽으로 이동시켜 나누어지는 값의 최상위 비트와 정렬합니다.
		BigIntegerUnit shiftedDivisor = absDivisor << bitLengthDifference;

		// 이 반복문은 비트 길이 차이에서 0까지 반복하여, 나누는 값을 오른쪽으로 이동시키면서 나눗셈을 수행합니다.
		for (int i = bitLengthDifference; i >= 0; i--)
		{
			// 이 조건문은 이동된 나누는 값이 나머지보다 작거나 같으면, 나머지에서 이를 빼고 몫에 해당하는 2의 거듭제곱 값을 더합니다.
			while(shiftedDivisor <= remainder)
			{
				remainder -= shiftedDivisor;
				quotient += (new BigIntegerUnit(1) << i);
			}
			// 이 라인은 다음 반복을 위해 나누는 값을 오른쪽으로 한 비트 이동시킵니다.
			shiftedDivisor = shiftedDivisor >> 1;
		}

		// 이 라인은 나누어지는 값과 나누는 값의 부호에 따라 결과를 음수로 만들지 여부를 결정하여 몫을 반환합니다.
		return isNegativeResult ? -quotient : quotient;
	}

	public static bool operator >(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit result = leftSide-rightSide;
		if (result.IsNegative || result.IsZero)
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
		return !(leftSide > rightSide);
	}

	public static bool operator ==(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		if (object.ReferenceEquals(leftSide, null) && object.ReferenceEquals(rightSide, null))
		{
			return true;
		}
		else if (object.ReferenceEquals(leftSide, null) || object.ReferenceEquals(rightSide, null))
		{
			return false;
		}
		else
		{
			return (leftSide-rightSide).IsZero;
		}
	}
	public static bool operator !=(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		return !(leftSide == rightSide);
	}

	public static BigIntegerUnit Abs(BigIntegerUnit value)
	{
		if (value.IsNegative)
		{
			return -value;
		}

		return value;
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
		int bitLength = GetHighestBitPosition(byteArray);

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
		if (newByteLength < 0)
		{
			return new BigIntegerUnit(0);
		}
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
			sb.Append($"{_value[i].ToString("X")} ");
		}
		return sb.ToString();
	}

}

