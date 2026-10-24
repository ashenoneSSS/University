module Main (main) where

import Control.Concurrent (getNumCapabilities, setNumCapabilities)
import Control.Exception (evaluate)
import Control.Monad (forM_, when)
import DiscreteLog
import GHC.Clock (getMonotonicTime)
import GHC.Conc (getNumProcessors)
import ModArith
import System.Environment (getArgs)
import System.Exit (exitFailure)
import System.IO (BufferMode (..), hFlush, hPutStrLn, hSetBuffering, hSetEncoding, stderr, stdin, stdout, utf8)
import Text.Printf (printf)
import Text.Read (readMaybe)

defaultProblem :: Problem
defaultProblem = Problem 2147483647 7 (powMod 7 987654321 2147483647)

main :: IO ()
main = do
  mapM_ (`hSetEncoding` utf8) [stdin, stdout, stderr]
  hSetBuffering stdout NoBuffering
  args <- getArgs
  cores <- getNumProcessors
  printf "Дискретне логарифмування: знайти x, для якого g^x ≡ h (mod p)\n"
  printf "Доступно процесорних ядер: %d\n\n" cores
  problem <- case args of
    [p, g, h] -> maybe (abort "Аргументи мають бути цілими числами: p g h") pure
                   (Problem <$> readMaybe p <*> readMaybe g <*> readMaybe h)
    _ -> askProblem
  validate problem
  threads <- askNumber ("Максимальна кількість потоків [" ++ show cores ++ "]: ") cores
  setNumCapabilities (max 1 threads)
  solveAll problem threads

abort :: String -> IO a
abort message = hPutStrLn stderr ("Помилка: " ++ message) >> exitFailure

askProblem :: IO Problem
askProblem = do
  putStrLn "Введіть p, g, h (Enter — приклад p = 2^31 - 1, g = 7, h = 7^987654321 mod p)."
  p <- askNumber "p: " (modulus defaultProblem)
  g <- askNumber "g: " (base defaultProblem)
  h <- askNumber "h: " (target defaultProblem)
  pure (Problem p g h)

askNumber :: String -> Int -> IO Int
askNumber label def = do
  putStr label
  hFlush stdout
  line <- getLine
  if null line
    then pure def
    else maybe (putStrLn "Потрібне ціле число." >> askNumber label def) pure (readMaybe line)

validate :: Problem -> IO ()
validate (Problem p g h) = do
  when (p < 3 || p > maxModulus) $
    abort ("p має бути в межах 3 .. " ++ show maxModulus)
  when (gcd g p /= 1 || gcd h p /= 1) $
    abort "g і h мають бути взаємно простими з p"
  printf "p = %d, g = %d, h = %d\n\n" p g (h `mod` p)

timed :: IO a -> IO (a, Double)
timed action = do
  start <- getMonotonicTime
  result <- action
  end <- getMonotonicTime
  pure (result, end - start)

threadCounts :: Int -> [Int]
threadCounts k = takeWhile (< k) (iterate (* 2) 1) ++ [k]

report :: Problem -> String -> Int -> (Maybe Int, Double) -> Double -> IO ()
report (Problem p g h) name k (answer, seconds) baseline = do
  let check = case answer of
        Just x | powMod g x p == h `mod` p -> "так"
        Just _ -> "НІ"
        Nothing -> "—"
  printf "%-28s %7d %12.4f %10.2f %14s %10s\n"
    name k seconds (baseline / seconds) (maybe "не існує" show answer) (check :: String)

solveAll :: Problem -> Int -> IO ()
solveAll problem k = do
  caps <- getNumCapabilities
  printf "%-28s %7s %12s %10s %14s %10s\n" "Метод" "Потоків" "Час, с" "Прискор." "x" "Перевірка"
  printf "%s\n" (replicate 86 '-')
  sequential <- timed (evaluate (bruteForce problem))
  report problem "Повний перебір (послідовно)" 1 sequential (snd sequential)
  forM_ (threadCounts k) $ \t -> do
    setNumCapabilities t
    result <- timed (bruteForceParallel t problem)
    report problem "Повний перебір (паралельно)" t result (snd sequential)
  bsgs <- timed (evaluate (babyGiant problem))
  report problem "Великий/малий крок (посл.)" 1 bsgs (snd bsgs)
  forM_ (threadCounts k) $ \t -> do
    setNumCapabilities t
    result <- timed (babyGiantParallel t problem)
    report problem "Великий/малий крок (пар.)" t result (snd bsgs)
  setNumCapabilities caps
