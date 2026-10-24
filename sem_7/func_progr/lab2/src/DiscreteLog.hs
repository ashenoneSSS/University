{-# LANGUAGE BangPatterns #-}

module DiscreteLog
  ( Problem (..)
  , bruteForce
  , bruteForceParallel
  , babyGiant
  , babyGiantParallel
  , chunks
  ) where

import Control.Concurrent (forkOn)
import Control.Concurrent.MVar (newEmptyMVar, putMVar, takeMVar)
import Control.Exception (evaluate)
import Control.Monad (forM)
import Data.IORef (IORef, atomicModifyIORef', newIORef, readIORef)
import qualified Data.IntMap.Strict as IM
import Data.List (foldl')
import ModArith

data Problem = Problem
  { modulus :: !Int
  , base    :: !Int
  , target  :: !Int
  } deriving (Show)

chunks :: Int -> Int -> Int -> [(Int, Int)]
chunks k lo hi =
  [ (lo + i * size + min i extra, lo + (i + 1) * size + min (i + 1) extra)
  | i <- [0 .. k - 1]
  , size > 0 || i < extra
  ]
  where
    total = hi - lo
    size = total `div` k
    extra = total `mod` k

order :: Problem -> Int
order p = modulus p - 1

bruteForce :: Problem -> Maybe Int
bruteForce (Problem m g h) = go 0 1
  where
    hm = h `mod` m
    go !i !cur
      | i >= m - 1 = Nothing
      | cur == hm = Just i
      | otherwise = go (i + 1) (mulMod cur g m)

scanStrided :: Problem -> IORef Int -> Int -> Int -> IO (Maybe Int)
scanStrided (Problem m g h) best k offset = go offset (powMod g offset m) (0 :: Int)
  where
    hm = h `mod` m
    step = powMod g k m
    limit = m - 1
    go !i !cur !n
      | i >= limit = pure Nothing
      | cur == hm = do
          atomicModifyIORef' best (\b -> (min b i, ()))
          pure (Just i)
      | n == 65536 = do
          b <- readIORef best
          if b < i then pure Nothing else go (i + k) (mulMod cur step m) 0
      | otherwise = go (i + k) (mulMod cur step m) (n + 1)

bruteForceParallel :: Int -> Problem -> IO (Maybe Int)
bruteForceParallel k p = do
  best <- newIORef maxBound
  found <- runParallel [scanStrided p best k offset | offset <- [0 .. k - 1]]
  pure (minimum' [x | Just x <- found])

minimum' :: [Int] -> Maybe Int
minimum' [] = Nothing
minimum' xs = Just (minimum xs)

stepCount :: Problem -> Int
stepCount p = ceiling (sqrt (fromIntegral (order p) :: Double))

babyTable :: Problem -> (Int, Int) -> IM.IntMap Int
babyTable (Problem m g _) (lo, hi) = go lo (powMod g lo m) IM.empty
  where
    go !j !cur !acc
      | j >= hi = acc
      | otherwise = go (j + 1) (mulMod cur g m) (IM.insertWith min cur j acc)

giantSteps :: Problem -> Int -> Int -> IM.IntMap Int -> (Int, Int) -> Maybe Int
giantSteps (Problem m _ h) n factor table (lo, hi) = go lo (mulMod (h `mod` m) (powMod factor lo m) m)
  where
    go !i !gamma
      | i >= hi = Nothing
      | otherwise = case IM.lookup gamma table of
          Just j -> Just (i * n + j)
          Nothing -> go (i + 1) (mulMod gamma factor m)

giantFactor :: Problem -> Int -> Maybe Int
giantFactor (Problem m g _) n = (\inv -> powMod inv n m) <$> invMod g m

babyGiant :: Problem -> Maybe Int
babyGiant p = do
  let n = stepCount p
  factor <- giantFactor p n
  giantSteps p n factor (babyTable p (0, n)) (0, n + 1)

babyGiantParallel :: Int -> Problem -> IO (Maybe Int)
babyGiantParallel k p =
  case giantFactor p n of
    Nothing -> pure Nothing
    Just factor -> do
      parts <- runParallel [evaluate (babyTable p r) | r <- chunks k 0 n]
      let table = foldl' (IM.unionWith min) IM.empty parts
      _ <- evaluate (IM.size table)
      answers <- runParallel [evaluate (giantSteps p n factor table r) | r <- chunks k 0 (n + 1)]
      pure (minimum' [x | Just x <- answers])
  where
    n = stepCount p

runParallel :: [IO a] -> IO [a]
runParallel actions = do
  boxes <- forM (zip [0 ..] actions) $ \(core, action) -> do
    box <- newEmptyMVar
    _ <- forkOn core (action >>= putMVar box)
    pure box
  mapM takeMVar boxes
