# Simple, thoroughly commented implementation of DES using Ruby
# Chris Hulbert - chris.hulbert@gmail.com - http://splinter.com.au/blog
# Reference: http://orlingrabbe.com/des.htm

class Array
  def perm(table)
    table.split(' ').map{ |bit| self[bit.to_i-1] }
  end

  def pc1
    perm "
      57   49    41   33    25    17    9
       1   58    50   42    34    26   18
      10    2    59   51    43    35   27
      19   11     3   60    52    44   36
      63   55    47   39    31    23   15
       7   62    54   46    38    30   22
      14    6    61   53    45    37   29
      21   13     5   28    20    12    4 "
  end
  
  def pc2
    perm "
      14    17   11    24     1    5
       3    28   15     6    21   10
      23    19   12     4    26    8
      16     7   27    20    13    2
      41    52   31    37    47   55
      30    40   51    45    33   48
      44    49   39    56    34   53
      46    42   50    36    29   32"
  end
  
  def ip
    perm "
      58    50   42    34    26   18    10    2
      60    52   44    36    28   20    12    4
      62    54   46    38    30   22    14    6
      64    56   48    40    32   24    16    8
      57    49   41    33    25   17     9    1
      59    51   43    35    27   19    11    3
      61    53   45    37    29   21    13    5
      63    55   47    39    31   23    15    7"
  end
  
  def e_bits
    perm "
      32     1    2     3     4    5
       4     5    6     7     8    9
       8     9   10    11    12   13
      12    13   14    15    16   17
      16    17   18    19    20   21
      20    21   22    23    24   25
      24    25   26    27    28   29
      28    29   30    31    32    1"
  end
  
  def perm_p
    perm "
      16   7  20  21
      29  12  28  17
       1  15  23  26
       5  18  31  10
       2   8  24  14
      32  27   3   9
      19  13  30   6
      22  11   4  25"
  end
  
  def ip_inverse
    perm "
      40     8   48    16    56   24    64   32
      39     7   47    15    55   23    63   31
      38     6   46    14    54   22    62   30
      37     5   45    13    53   21    61   29
      36     4   44    12    52   20    60   28
      35     3   43    11    51   19    59   27
      34     2   42    10    50   18    58   26
      33     1   41     9    49   17    57   25"
  end
  
  def s_box(b)
    s_tables = "
                             S1

     14  4  13  1   2 15  11  8   3 10   6 12   5  9   0  7
      0 15   7  4  14  2  13  1  10  6  12 11   9  5   3  8
      4  1  14  8  13  6   2 11  15 12   9  7   3 10   5  0
     15 12   8  2   4  9   1  7   5 11   3 14  10  0   6 13

                             S2

     15  1   8 14   6 11   3  4   9  7   2 13  12  0   5 10
      3 13   4  7  15  2   8 14  12  0   1 10   6  9  11  5
      0 14   7 11  10  4  13  1   5  8  12  6   9  3   2 15
     13  8  10  1   3 15   4  2  11  6   7 12   0  5  14  9

                             S3

     10  0   9 14   6  3  15  5   1 13  12  7  11  4   2  8
     13  7   0  9   3  4   6 10   2  8   5 14  12 11  15  1
     13  6   4  9   8 15   3  0  11  1   2 12   5 10  14  7
      1 10  13  0   6  9   8  7   4 15  14  3  11  5   2 12

                             S4

      7 13  14  3   0  6   9 10   1  2   8  5  11 12   4 15
     13  8  11  5   6 15   0  3   4  7   2 12   1 10  14  9
     10  6   9  0  12 11   7 13  15  1   3 14   5  2   8  4
      3 15   0  6  10  1  13  8   9  4   5 11  12  7   2 14

                             S5

      2 12   4  1   7 10  11  6   8  5   3 15  13  0  14  9
     14 11   2 12   4  7  13  1   5  0  15 10   3  9   8  6
      4  2   1 11  10 13   7  8  15  9  12  5   6  3   0 14
     11  8  12  7   1 14   2 13   6 15   0  9  10  4   5  3

                             S6

     12  1  10 15   9  2   6  8   0 13   3  4  14  7   5 11
     10 15   4  2   7 12   9  5   6  1  13 14   0 11   3  8
      9 14  15  5   2  8  12  3   7  0   4 10   1 13  11  6
      4  3   2 12   9  5  15 10  11 14   1  7   6  0   8 13

                             S7

      4 11   2 14  15  0   8 13   3 12   9  7   5 10   6  1
     13  0  11  7   4  9   1 10  14  3   5 12   2 15   8  6
      1  4  11 13  12  3   7 14  10 15   6  8   0  5   9  2
      6 11  13  8   1  4  10  7   9  5   0 15  14  2   3 12

                             S8

     13  2   8  4   6 15  11  1  10  9   3 14   5  0  12  7
      1 15  13  8  10  3   7  4  12  5   6 11   0 14   9  2
      7 11   4  1   9 12  14  2   0  6  10 13  15  3   5  8
      2  1  14  7   4 10   8 13  15 12   9  0   3  5   6 11
      "
    s_table = s_tables[s_tables.index('S%d'%b)+3,999]
    s_table = s_table[0,s_table.index('S')] if s_table.index('S')
    s_table = s_table.split(' ')
    row = self.first*2 + self.last
    col = self[1]*8 + self[2]*4 + self[3]*2 + self[4]
    s_table[row*16+col].to_i.to_bits
  end
  
  # split this array into two halves
  def halves
    [self[0,self.length/2], self[self.length/2,self.length/2]]
  end
  
  # join this array into a nicely grouped string
  def pretty(n=8)
    s=""
    self.each_with_index{|bit,i| s+=bit.to_s; s+=' ' if (i+1)%n==0}
    s
  end
  
  # shift this array one or two bits left
  def left(n)
    self[n,self.length] + self[0,n]
  end
  
  # xor's this and the other array
  def xor(b)
    i=0
    self.map{|a| i+=1; a^b[i-1]}
  end
  
  # splits into arrays of 6 bits
  def split6
    arr=[]
    subarr=[]
    self.each{|a|
      subarr<<a
      if subarr.length==6
        arr<<subarr
        subarr=[]
      end
    }
    arr
  end
end

class String
  def to_bits
    bitarr=[]
    self.each_char{|c| bitarr << c.to_i if c=='0' || c=='1'}
    bitarr
  end
end

class Integer
  def to_bits
    [self>>3, (self>>2)&1, (self>>1)&1, self&1]
  end
end

def shifts(c0,d0)
  shift_numbers = %w"
    1
    1
    2
    2
    2
    2
    2
    2
    1
    2
    2
    2
    2
    2
    2
    1"
  last_c, last_d = c0, d0
  shift_numbers.map{|n|
    new_c = last_c.left(n.to_i)
    new_d = last_d.left(n.to_i)
    last_c = new_c
    last_d = new_d
    new_c + new_d
  }
end

def f(r,k)
  e = r.e_bits
  x = e.xor(k)
  bs = x.split6
  s = []
  bs.each_with_index{|b,i| s += b.s_box(i+1)}
  s.perm_p
end

# Step 1, make the subkeys
k = '00010011 00110100 01010111 01111001 10011011 10111100 11011111 11110001'.to_bits
kplus = k.pc1
c0, d0 = kplus.halves
cdn = shifts(c0, d0) # maybe just make this simpler?
subkeys = cdn.map{|cd| cd.pc2}

# Step 2, encode it
m = '0000 0001 0010 0011 0100 0101 0110 0111 1000 1001 1010 1011 1100 1101 1110 1111'.to_bits
ip = m.ip
l, r = ip.halves
(0..15).each { |i|
  l, r = r, l.xor(f(r,subkeys[i]))
}
rl = r + l
c = rl.ip_inverse

puts c.pretty 8