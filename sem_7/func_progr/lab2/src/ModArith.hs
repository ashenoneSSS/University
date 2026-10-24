{-# LANGUAGE BangPatterns #-}

module ModArith
  ( mulMod
  , powMod
  , invMod
  , maxModulus
  ) where

maxModulus :: Int
maxModulus = 3037000499

mulMod :: Int -> Int -> Int -> Int
mulMod a b m = (a * b) `rem` m
{-# INLINE mulMod #-}

powMod :: Int -> Int -> Int -> Int
powMod base expo m = go (base `mod` m) expo 1
  where
    go !_ 0 !acc = acc `rem` m
    go !b !e !acc
      | odd e = go (mulMod b b m) (e `div` 2) (mulMod acc b m)
      | otherwise = go (mulMod b b m) (e `div` 2) acc

invMod :: Int -> Int -> Maybe Int
invMod a m =
  let (g, x, _) = egcd (a `mod` m) m
   in if g /= 1 then Nothing else Just (x `mod` m)
  where
    egcd 0 b = (b, 0, 1)
    egcd x y =
      let (d, s, t) = egcd (y `mod` x) x
       in (d, t - (y `div` x) * s, s)
