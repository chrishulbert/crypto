// Simple, thoroughly commented implementation of RSA using C#
// Chris Hulbert - chris.hulbert@gmail.com - http://splinter.com.au/blog
// http://github.com/chrishulbert/crypto

using System;
using System.Text;
using System.Collections.Generic;

class SimpleBignum
{
  /// <summary>
  /// The number of digits we use for each bignum number. 1024 bytes should mean we can do up to 4096-bit RSA.
  /// </summary>
  const int MaxDigits = 1024;

  /// <summary>
  /// The digits that make up this big number. The first one is the least significant. Each digit is 0-255,
  /// so it as if we are doing base-256 maths using this class.
  /// </summary>
  byte[] digits = new byte[MaxDigits];

  /// <summary>
  /// Creates a new, zero, bignum
  /// </summary>
  public SimpleBignum()
  {
  }

  /// <summary>
  /// Creates a new bignum by copying the value of an existing one
  /// </summary>
  public SimpleBignum(SimpleBignum source)
  {
    Array.Copy(source.digits, digits, MaxDigits);
  }

  /// <summary>
  /// Creates a new bignum from a hex string
  /// </summary>
  public SimpleBignum(string str)
  {
    int bytes = str.Length / 2;
    for (int i = 0; i < bytes; i++)
      digits[bytes - i - 1] = (byte)Convert.ToInt32(str.Substring(i * 2, 2), 16);
  }

  /// <summary>
  /// Adds two big nums, using the school method
  /// </summary>
  public static SimpleBignum operator +(SimpleBignum a, SimpleBignum b)
  {
    SimpleBignum result = new SimpleBignum();
    int carry = 0;
    for (int i = 0; i < MaxDigits; i++)
    {
      int newval = a.digits[i] + b.digits[i] + carry;
      result.digits[i] = (byte)(newval % 256);
      carry = (byte)(newval / 256);
    }
    return result;
  }

  /// <summary>
  /// Do a - b, undefined if b is bigger
  /// </summary>
  public static SimpleBignum operator -(SimpleBignum a, SimpleBignum b)
  {
    SimpleBignum result = new SimpleBignum();
    // Basically go from least significant to most significant, subtracting a digit at a time, and
    // borrowing 1 from the next digit if the result is negative. Just like the pen + paper method.
    int borrow = 0;
    for (int i = 0; i < MaxDigits; i++)
    {
      int newval = a.digits[i] - b.digits[i] - borrow;
      if (newval < 0)
      {
        newval += 256;
        borrow = 1;
      }
      else
      {
        borrow = 0;
      }
      result.digits[i] = (byte)newval;
    }
    return result;
  }

  /// <summary>
  /// Multiply a bignum by 1 digit
  /// </summary>
  SimpleBignum Mult1(int mult)
  {
    SimpleBignum result = new SimpleBignum();
    int carry = 0;
    for (int i = 0; i < MaxDigits; i++)
    {
      int newval = digits[i] * mult + carry;
      result.digits[i] = (byte)(newval % 256);
      carry = (byte)(newval / 256);
    }
    return result;
  }

  /// <summary>
  /// Shifts left by a whole number of digits (eg *8 bits), making the number bigger.
  /// In implementation this shifts the array *right*, because we store the least significant byte first.
  /// </summary>
  void ShiftLeftDigits(int d)
  {
    Array.Copy(digits, 0, digits, d, MaxDigits - d); // Move the digits along
    Array.Clear(digits, 0, d); // Clear the digits that were moved
  }

  /// <summary>
  /// Multiplies using the method you were taught at school. Not the fastest but it'll do.
  /// Eg multiply by each shifted digit individually, totalling as it goes
  /// </summary>
  public static SimpleBignum operator *(SimpleBignum a, SimpleBignum b)
  {
    SimpleBignum result = new SimpleBignum();
    SimpleBignum temp; // This is used to store the multiplication by each digit
    for (int i = 0; i < MaxDigits; i++)
    {
      if (b.digits[i] > 0)
      { // Save time by skipping multiplying by zero columns
        temp = a.Mult1(b.digits[i]); // temp = a * single-digit-from-b
        temp.ShiftLeftDigits(i); // temp is shifted to line up with the column we're using from b
        result += temp; // Add temp to the running total
      }
    }
    return result;
  }

  /// <summary>
  /// Is a >= b ?
  /// </summary>
  public static bool operator >=(SimpleBignum a, SimpleBignum b)
  {
    // First up check if equal
    bool same = true;
    for (int i = 0; i < MaxDigits; i++)
    {
      if (a.digits[i] != b.digits[i])
      {
        same = false;
        break;
      }
    }
    if (same) return true;

    // Now subtract b-a to find which is bigger
    int borrow = 0;
    for (int i = 0; i < MaxDigits; i++)
    {
      int newval = b.digits[i] - a.digits[i] - borrow;
      if (newval < 0)
      {
        borrow = 1;
      }
      else
      {
        borrow = 0;
      }
    }
    return borrow > 0;
  }

  /// <summary>
  /// Need to implement this for c#'s rules
  /// </summary>
  public static bool operator <=(SimpleBignum a, SimpleBignum b)
  {
    return b >= a;
  }

  /// <summary>
  /// Shifts the current bignum right one bit (effectively halving it)
  /// </summary>
  public void ShiftRight1Bit()
  {
    for (int i = 0; i < MaxDigits - 1; i++)
    {
      digits[i] = (byte)((digits[i] >> 1) + ((digits[i + 1] & 1) << 7)); // Shift this one right, plus borrow a bit from the next
    }
    digits[MaxDigits - 1] >>= 1; // Sort out the last byte (it can't borrow a bit)
  }

  /// <summary>
  /// Finds the remainder of a/b, using the shift and subtract method
  /// </summary>
  public static SimpleBignum operator %(SimpleBignum a, SimpleBignum b)
  {
    SimpleBignum result = new SimpleBignum(a); // Start off with out=a, and whittle it down 
    int len_a = a.SignificantBytes; // Get the lengths
    int len_b = b.SignificantBytes;
    if (len_b > len_a) return result; // Simple case: since b is bigger, a is already the modulus
    int byte_shifts = len_a - len_b + 1; // Figure out how many shifts needed to make b bigger than a
    SimpleBignum shifted = new SimpleBignum(b); // shifted = b
    shifted.ShiftLeftDigits(byte_shifts); // Now b is shifted bigger than a
    // Now do a series of bit shifts on B, subtracting it from A each time
    for (int i = 0; i < byte_shifts * 8; i++)
    {
      shifted.ShiftRight1Bit();

      if (result >= shifted)
        result -= shifted;
    }
    return result;
  }

  /// <summary>
  /// Power Modulus: this ^ power % mod
  /// This does modular exponentiation using the right-to-left binary method
  /// This is actually quite slow, mainly due to the mod function, but also the mult is slow too
  /// Clobbers power
  /// </summary>
  public SimpleBignum PowMod(SimpleBignum power, SimpleBignum mod)
  {
    SimpleBignum result = new SimpleBignum("01"); // result = 1
    SimpleBignum baseNum = new SimpleBignum(this); // Make a copy of this

    while (power.GreaterThanZero) // while power > 0
    {
      if ((power.digits[0] & 1) == 1) // If lowest bit is set
        result = (result * baseNum) % mod; // result = result*base % mod

      baseNum = (baseNum * baseNum) % mod; // base = base^2 % mod

      power.ShiftRight1Bit(); // power>>=1
    }

    return result;
  }

  /// <summary>
  /// Gets the number of significant bytes in this bignumber
  /// </summary>
  public int SignificantBytes
  {
    get
    {
      int len = 1; // Look for the highest nonzero byte
      for (int i = 0; i < MaxDigits; i++)
        if (digits[i] > 0)
          len = i + 1;
      return len;
    }
  }

  /// <summary>
  /// Returns true if this > 0
  /// </summary>
  public bool GreaterThanZero
  {
    get
    {
      foreach (byte b in digits) // Check all digits for a nonzero one
        if (b > 0) return true;
      return false;
    }
  }

  /// <summary>
  /// Converts this to a hex string
  /// </summary>
  public override string ToString()
  {
    string str = "";
    for (int i = 0; i < SignificantBytes; i++)
      str = digits[i].ToString("x2") + str;
    return str;
  }

  /// <summary>
  /// Creates a bignum by converting a text string into each character's ascii code, and then to a big num
  /// Eg ABC = 0x41 42 43
  /// </summary>
  public static SimpleBignum FromText(string str)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(str); // Convert from string to bytes
    string hex = BitConverter.ToString(bytes).Replace("-", ""); // Convert from bytes to hex string
    return new SimpleBignum(hex); // Get the initialiser to convert the hex string to a bignum
  }

  /// <summary>
  /// Converts the current bignum to a string eg if the bignum is 0x414243, it becomes "ABC" because
  /// 0x41 = A, 0x42 = B, 0x43 = C
  /// </summary>
  public string ToText()
  {
    // Create a byte[] with the most significant first, by reversing the digits array
    int len = SignificantBytes;
    byte[] arr = new byte[len];
    for (int i = 0; i < len; i++)
    {
      arr[len - 1 - i] = digits[i];
    }
    // Now just convert that byte[] to a string
    return Encoding.UTF8.GetString(arr);
  }
}

class Program
{
  /// <summary>
  /// Test the crypto
  /// </summary>
  static void Main(string[] args)
  {
    Console.WriteLine("Testing RSA");

    //SimpleBignum p = new SimpleBignum("dd755ca44f2f399a845690ef8507365befef9505fbf416cb0b306bd13221a00368e8bd45f7357d2686b8437816da326dc40b7c756d2407bb9a8c8a3fb2b8e79d");
    //SimpleBignum q = new SimpleBignum("e083f0abbd0bee477bc86aa12077b82b5f7a035ac614dd494fa55a57e03deea0527f54e31e715374b3dd992ea9f80bb94b7e3b2b4e02e8901af79e688c1c7483");
    //SimpleBignum m = new SimpleBignum("5468697320697320676f696e6720746f20626520656d626172726173696e6720696620697420646f65736e277420776f726b21");
    //SimpleBignum c = new SimpleBignum("68c1a28435c90c20e3e0111302f97222c875215ce37178cdca30fbd90fceafaa7aa90c5d0dee2290a3b4cf944a177175acd5cb29cb03869bce2b4f93357cb94b08f8f1f08f793f9a7015338be19ff6b9301aa144665ffe0f7749885d3c3a51f8627d1e26ad629525eee59da7d5c69fe2926b6fb51ded336b6033a203d1ef5bc3");
    SimpleBignum e = new SimpleBignum("010001");
    SimpleBignum n = new SimpleBignum("c238d450c526bb2014b1489505540eb8330c7e01ed7ac4a7d9a52423025f9bdd5eb42b2103b6a069e43678bef68fa67703c304c590c6629bd455f4d8c0a145599df37bdefa19b52532937a2ccc22fb36f73c6dad819bc01e1326028fab37a052e0efae05e437573f2254a5ea4a43d1f3dbec2b22bf24fc6dddd0443f6ebda957");
    SimpleBignum d = new SimpleBignum("305bf211826558666e808deffcf9a7089a3d5c0aa2d4d4ae6e74be00b19098c08fda107b11efa1157cab4b7950ef07a5ce9bfa4e2ef4168d725b4cb1c394e42d332999fa20a42f4c31fdeba079c6931a11915f66d2b47c75571d334ce075bc417df8bc0848ae97b7abf6472ab7c83de2da691115a864d32496200d26a1d91791");

    string msg = "This is going to be embarrasing if it doesn't work!";
    SimpleBignum m = SimpleBignum.FromText(msg);
    Console.WriteLine("Message (text): {0}", msg);
    Console.WriteLine("Message (num):  {0}", m);

    Console.WriteLine("Encrypting (should be 68c1...5bc3):");
    SimpleBignum c = m.PowMod(e, n);
    Console.WriteLine("Encrypted: {0}", c);

    Console.WriteLine("Decrypting (please wait, this algorithm isn't optimised):");
    SimpleBignum a = c.PowMod(d, n);
    string aText = a.ToText();
    Console.WriteLine("Decrypted (num):  {0}", a);
    Console.WriteLine("Decrypted (text): {0}", aText);
  }
}