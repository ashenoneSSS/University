module Input
  ( ask
  , askDefault
  , askInt
  , askIntDefault
  , askIntRange
  , askDay
  , askDayDefault
  , askMaybeDay
  , askMaybe
  , askIds
  ) where

import Data.Char (isSpace)
import Data.Time (Day, defaultTimeLocale, parseTimeM)
import System.IO (hFlush, stdout)
import Text.Read (readMaybe)

trim :: String -> String
trim = f . f
  where f = reverse . dropWhile isSpace

ask :: String -> IO String
ask label = do
  putStr (label ++ ": ")
  hFlush stdout
  value <- trim <$> getLine
  if null value then ask label else pure value

askDefault :: String -> String -> IO String
askDefault label current = do
  putStr (label ++ " [" ++ current ++ "]: ")
  hFlush stdout
  value <- trim <$> getLine
  pure (if null value then current else value)

askMaybe :: String -> Maybe String -> IO (Maybe String)
askMaybe label current = do
  putStr (label ++ " [" ++ maybe "-" id current ++ "] (\"-\" — очистити): ")
  hFlush stdout
  value <- trim <$> getLine
  pure $ case value of
    ""  -> current
    "-" -> Nothing
    v   -> Just v

askInt :: String -> IO Int
askInt label = do
  value <- ask label
  maybe (putStrLn "  Потрібне ціле число." >> askInt label) pure (readMaybe value)

askIntDefault :: String -> Int -> IO Int
askIntDefault label current = do
  value <- askDefault label (show current)
  maybe (putStrLn "  Потрібне ціле число." >> askIntDefault label current) pure (readMaybe value)

askIntRange :: String -> Int -> Int -> IO Int
askIntRange label lo hi = do
  n <- askInt (label ++ " (" ++ show lo ++ "–" ++ show hi ++ ")")
  if n >= lo && n <= hi then pure n else putStrLn "  Значення поза межами." >> askIntRange label lo hi

parseDay :: String -> Maybe Day
parseDay = parseTimeM True defaultTimeLocale "%Y-%m-%d"

askDay :: String -> IO Day
askDay label = do
  value <- ask (label ++ " (РРРР-ММ-ДД)")
  maybe (putStrLn "  Невірний формат дати." >> askDay label) pure (parseDay value)

askDayDefault :: String -> Day -> IO Day
askDayDefault label current = do
  value <- askDefault (label ++ " (РРРР-ММ-ДД)") (show current)
  maybe (putStrLn "  Невірний формат дати." >> askDayDefault label current) pure (parseDay value)

askMaybeDay :: String -> Maybe Day -> IO (Maybe Day)
askMaybeDay label current = do
  value <- askMaybe (label ++ " (РРРР-ММ-ДД)") (show <$> current)
  case value of
    Nothing -> pure Nothing
    Just v -> maybe (putStrLn "  Невірний формат дати." >> askMaybeDay label current) (pure . Just) (parseDay v)

askIds :: String -> [Int] -> IO [Int]
askIds label current = do
  value <- askDefault (label ++ " (через кому)") (show' current)
  case mapM (readMaybe . trim) (splitComma value) of
    Just ids -> pure ids
    Nothing  -> putStrLn "  Потрібні числа через кому." >> askIds label current
  where
    show' = foldr (\x acc -> if null acc then show x else show x ++ "," ++ acc) ""
    splitComma s = case break (== ',') s of
      (a, [])     -> [a]
      (a, _ : bs) -> a : splitComma bs
