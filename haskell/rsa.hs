{-
  Really simple RSA implementation in Haskell
  Chris Hulbert - chris.hulbert@gmail.com - http://github.com/chrishulbert/crypto
-}
import Data.Char
import Data.Bits
import Numeric

{- Output a bignum to the screen as hex -}
pretty i = putStrLn $ showHex i ""

{- Modular Exponentiation: b^e mod m -}
modExp2 b e m r
  | e == 0       = r
  | e .&. 1 == 1 = modExp2 (b*b `mod` m) (e `div` 2) m (r*b `mod` m)
  | otherwise    = modExp2 (b*b `mod` m) (e `div` 2) m r
modExp b e m = modExp2 b e m 1

{- Cleartext message M -}
m = toInteger $ 0x5468697320697320676f696e6720746f20626520656d626172726173696e6720696620697420646f65736e277420776f726b21
{- Public exponent E -}
e = toInteger $ 0x010001
{- Public key N (modulus) -}
n = toInteger $ 0xc238d450c526bb2014b1489505540eb8330c7e01ed7ac4a7d9a52423025f9bdd5eb42b2103b6a069e43678bef68fa67703c304c590c6629bd455f4d8c0a145599df37bdefa19b52532937a2ccc22fb36f73c6dad819bc01e1326028fab37a052e0efae05e437573f2254a5ea4a43d1f3dbec2b22bf24fc6dddd0443f6ebda957
{- Secret key D (exponent) -}
d = toInteger $ 0x305bf211826558666e808deffcf9a7089a3d5c0aa2d4d4ae6e74be00b19098c08fda107b11efa1157cab4b7950ef07a5ce9bfa4e2ef4168d725b4cb1c394e42d332999fa20a42f4c31fdeba079c6931a11915f66d2b47c75571d334ce075bc417df8bc0848ae97b7abf6472ab7c83de2da691115a864d32496200d26a1d91791
{- Cryptotext C (encrypted message) -}
c = toInteger $ 0x68c1a28435c90c20e3e0111302f97222c875215ce37178cdca30fbd90fceafaa7aa90c5d0dee2290a3b4cf944a177175acd5cb29cb03869bce2b4f93357cb94b08f8f1f08f793f9a7015338be19ff6b9301aa144665ffe0f7749885d3c3a51f8627d1e26ad629525eee59da7d5c69fe2926b6fb51ded336b6033a203d1ef5bc3

{- The RSA algorithms are actually quite simple as you can see -}
encrypt = modExp m e n
decrypt = modExp c d n

{- Test it all out -}
main = do
  putStrLn "Simple RSA crypto"
  putStrLn "Encrypt (should be 68c1...5bc3):"
  pretty encrypt
  putStrLn "Decrypt (should be 5468...6b21):"
  pretty decrypt
