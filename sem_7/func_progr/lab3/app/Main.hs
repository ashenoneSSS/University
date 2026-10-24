module Main (main) where

import Control.Exception (IOException, try)
import Parser
import Prelude hiding (Word)
import System.Environment (getArgs)
import System.Exit (exitFailure)
import System.IO
import Tasks
import TextModel

main :: IO ()
main = do
  mapM_ (`hSetEncoding` utf8) [stdin, stdout, stderr]
  args <- getArgs
  let path = case args of
        (p : _) -> p
        [] -> "textbook.txt"
  result <- try (readUtf8 path)
  case result of
    Left err -> do
      hPutStrLn stderr ("Не вдалося прочитати файл " ++ path ++ ": " ++ show (err :: IOException))
      exitFailure
    Right raw -> process path raw

readUtf8 :: FilePath -> IO String
readUtf8 path = withFile path ReadMode $ \h -> do
  hSetEncoding h utf8
  contents <- hGetContents h
  length contents `seq` pure contents

process :: FilePath -> String -> IO ()
process path raw = do
  let text@(Text sentences) = parseText raw
      allWords = wordsOf text
      sorted = alphabeticalWords text
  putStrLn ("Файл: " ++ path)
  putStrLn ("Речень: " ++ show (length sentences) ++ ", слів: " ++ show (length allWords)
            ++ ", різних слів: " ++ show (length sorted))
  putStrLn ""
  putStrLn "Текст після заміни табуляцій і послідовностей пробілів одним пробілом:"
  putStrLn (normalizeSpaces raw)
  putStrLn ""
  putStrLn "Слова тексту в алфавітному порядку (кожна нова літера — з червоного рядка):"
  mapM_ putStrLn (formatParagraphs 80 (groupByInitial sorted))
