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
	public readonly int Bits;

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
		
		Bits = GetHighestBitPosition(_value);
	}
	public BigIntegerUnit(List<byte> array)
	{
		_value = array.ToArray();
		
		Bits = GetHighestBitPosition(_value);
	}
	public BigIntegerUnit(BigIntegerUnit value)
	{
		_value = new byte[value.Count];
		Array.Copy(value.Value, 0, _value, 0, _value.Length);
		
		Bits = GetHighestBitPosition(_value);

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
		if (isNagtive) _value = ToTwosComplement(_value);
		
		Bits = GetHighestBitPosition(_value);
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
		Bits = GetHighestBitPosition(_value);
	}
	public BigIntegerUnit(long value)
	{
		bool isNagtive = value < 0;
		if (isNagtive)
		{
			value = -value;
		}
		int size = sizeof(long);
		_value = new byte[size];
		for (int i = 0; i < size; i++)
		{
			_value[i] = (byte)(value >> BytePerBit * i);
		}
		_value = TrimZeros(_value);
		if (isNagtive) _value = ToTwosComplement(_value);
		
		Bits = GetHighestBitPosition(_value);
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
		Bits = GetHighestBitPosition(_value);
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
			BigIntegerUnit negativeUnit = new BigIntegerUnit(ToTwosComplement(positiveArray));
			_value = new byte[negativeUnit.Count];
			Array.Copy(negativeUnit.Value, 0, _value, 0, negativeUnit.Count);
		}
		else
		{
			_value = positiveArray;
		}
		
		Bits = GetHighestBitPosition(_value);
	}

	// O(N)
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
	// O(N)
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
		
		byte[] result = new byte[noZeroIndex+2];
		Array.Copy(onlyPositiveArray, 0, result, 0, noZeroIndex + 1);
		return result;
	}

	public static BigIntegerUnit ToTwosComplement(BigIntegerUnit value)
	{
		return new BigIntegerUnit(ToTwosComplement(value.Value));
	}
	// O(N)
	private static byte[] ToTwosComplement(byte[] value)
	{
		byte[] result = new byte[value.Length];
		for (int i = 0; i < result.Length; i++)
		{
			result[i] = (byte)(~value[i]);
		}

		int sum = 1;
		for (int i = 0; i < result.Length; i++)
		{
			sum += result[i];
			result[i] = (byte)(sum);
			sum = sum >> BytePerBit;
		}
		return result;
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
	public static BigIntegerUnit operator +(int leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit leftSideUnit = new BigIntegerUnit(leftSide);
		return leftSideUnit + rightSide;
	}

	public static BigIntegerUnit operator ++(BigIntegerUnit leftSide)
	{
		return leftSide + 1;
	}
	public static BigIntegerUnit operator --(BigIntegerUnit leftSide)
	{
		return leftSide - 1;
	}
	
	public static BigIntegerUnit operator -(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		BigIntegerUnit right = ToTwosComplement(rightSide);
		return leftSide + right;
	}
	public static BigIntegerUnit operator -(BigIntegerUnit leftSide, int rightSide)
	{
		BigIntegerUnit right = ToTwosComplement(new BigIntegerUnit(rightSide));
		return leftSide + right;
	}
	public static BigIntegerUnit operator -(BigIntegerUnit leftSide)
	{
		return ToTwosComplement(leftSide);
	}

	// O(N^2)
	public static BigIntegerUnit operator *(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		// 최적화 TODO
		// 010101101010
		// 001011010010
		// 1들을 찾아서 << 자리수 만큼
		// 그것들을 모두 합 -> 근데 그걸 내가만든 합으로 해야해서 느릴듯.
		
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
		return Divide(dividend, divisor, out BigIntegerUnit result);
	}
	public static BigIntegerUnit operator /(BigIntegerUnit dividend, int divisor)
	{
		return Divide(dividend, new BigIntegerUnit(divisor), out BigIntegerUnit result);
	}
	public static BigIntegerUnit operator %(BigIntegerUnit dividend, BigIntegerUnit divisor)
	{
		Divide(dividend, divisor, out BigIntegerUnit result);
		return result;
	}
	public static BigIntegerUnit operator %(BigIntegerUnit dividend, int divisor)
	{
		Divide(dividend, new BigIntegerUnit(divisor), out BigIntegerUnit result);
		return result;
	}
	
	private static BigIntegerUnit Divide(BigIntegerUnit dividend, BigIntegerUnit divisor, out BigIntegerUnit remainder)
	{
		if (divisor.IsZero)
		{
			throw new DivideByZeroException("Cannot divide by zero.");
		}
		
		bool isNegativeResult = dividend.IsNegative != divisor.IsNegative;

		BigIntegerUnit absDividend = dividend.IsNegative ? -dividend : dividend;
		BigIntegerUnit absDivisor = divisor.IsNegative ? -divisor : divisor;
		
		BigIntegerUnit quotient = new BigIntegerUnit(0);
		remainder = new BigIntegerUnit(absDividend);
		
		if (dividend.IsZero)
		{
			return new BigIntegerUnit(0);
		}
		if (absDivisor > absDividend)
		{
			return new BigIntegerUnit(0);
		}
		
		int dividendHighestBit = absDividend.Bits; // 가장 마지막 바이트에서 가장 높은 비트 위치를 구함
		int divisorHighestBit = absDivisor.Bits; // 가장 마지막 바이트에서 가장 높은 비트 위치를 구함

		int bitLengthDifference = absDividend.Bits - absDivisor.Bits;


		BigIntegerUnit shiftedDivisor = absDivisor << bitLengthDifference;

		for (int i = bitLengthDifference; i >= 0; i--)
		{
			while(shiftedDivisor <= remainder)
			{
				remainder -= shiftedDivisor;
				quotient += (new BigIntegerUnit(1) << i);
			}
			shiftedDivisor = shiftedDivisor >> 1;
		}

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

	public override bool Equals(object? obj)
	{
		if (object.ReferenceEquals(obj, null))
		{
			return false;
		}
		else
		{
			return (BigIntegerUnit) obj == this;
		}
	}

	public static bool operator !=(BigIntegerUnit leftSide, BigIntegerUnit rightSide)
	{
		return !(leftSide == rightSide);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
	
	public static int ToInt32(BigIntegerUnit value)
	{
		if (object.ReferenceEquals(value, null))
		{
			throw new ArgumentNullException("value");
		}
		int result = 0;
		int temp = 0;
		for (int i = 0; i < 32/BytePerBit; i++)
		{
			if (i < value.Count)
			{
				temp = value.Value[i];
				result |= temp << i*BytePerBit;
			}
			else
			{
				break;
			}
		}

		return result;
	}
	public static uint ToUInt32(BigIntegerUnit value)
	{
		if (object.ReferenceEquals(value, null))
		{
			throw new ArgumentNullException("value");
		}
		uint result = 0;
		uint temp = 0;
		for (int i = 0; i < 32/BytePerBit; i++)
		{
			if (i < value.Count)
			{
				temp = value.Value[i];
				result |= temp << i*BytePerBit;
			}
			else
			{
				break;
			};
		}

		return result;
	}

	public static long ToInt64(BigIntegerUnit value)
	{
		if (object.ReferenceEquals(value, null))
		{
			throw new ArgumentNullException("value");
		}
		long result = 0;
		long temp = 0;
		for (int i = 0; i < 64/BytePerBit; i++)
		{
			if (i < value.Count)
			{
				temp = value.Value[i];
				result |= temp << i*BytePerBit;
			}
			else
			{
				break;
			}
		}

		return result;
	}
	public static ulong ToUInt64(BigIntegerUnit value)
	{
		if (object.ReferenceEquals(value, null))
		{
			throw new ArgumentNullException("value");
		}
		ulong result = 0;
		ulong temp = 0;
		for (int i = 0; i < 64/BytePerBit; i++)
		{
			if (i < value.Count)
			{
				temp = value.Value[i];
				result |= temp << i*BytePerBit;
			}
			else
			{
				break;
			}
		}

		return result;
	}
	
	public static BigIntegerUnit Abs(BigIntegerUnit value)
	{
		if (value.IsNegative)
		{
			return -value;
		}

		return value;
	}
	public BigIntegerUnit Abs()
	{
		if (IsNegative)
		{
			return ToTwosComplement(this);
		}
		else
		{
			return new BigIntegerUnit(this);
		}
	}
	
	private static byte[] CopyArrayFrom(BigIntegerUnit unit)
	{
		byte[] result = new byte[unit.Count];
		Array.Copy(unit.Value, 0, result, 0, unit.Count);
		return result;
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

	public BigIntegerUnit CopyTo()
	{
		byte[] result = new byte[this.Count];
		for (int i = 0; i < this.Count; i++)
		{
			result[i] = this.Value[i];
		}
		return new BigIntegerUnit(result);
	}
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < Count; i++)
		{
			sb.Append($"{_value[i].ToString()} ");
		}
		return sb.ToString();
	}

	public string ToHexString()
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < Count; i++)
		{
			sb.Append($"{_value[i].ToString("X")} ");
		}
		return sb.ToString();
	}
	public string ToDexString()
	{
		if (Count < sizeof(int))
		{
			if (IsNegative)
			{
				return ToInt32(this).ToString();
			}
			else
			{
				return ToUInt32(this).ToString();
			}
		}

		if (Count < sizeof(long))
		{
			if (IsNegative)
			{
				return ToInt64(this).ToString();
			}
			else
			{
				return ToUInt64(this).ToString();
			}
		}

		bool negativeResult = false;
		StringBuilder resultText = new StringBuilder();
		BigIntegerUnit unit = CopyTo();
		if (IsNegative)
		{
			unit = -unit;
			negativeResult = true;
		}
		BigIntegerUnit zero = new BigIntegerUnit(0);
		while (unit > zero)
		{
			resultText.Append(ToInt32((unit%10)).ToString());
			unit /= 10;
		}
		if(negativeResult){resultText.Append("-");}
		char[] text = resultText.ToString().ToCharArray();
		Array.Reverse(text);
		return new string(text);
	}
	
}

