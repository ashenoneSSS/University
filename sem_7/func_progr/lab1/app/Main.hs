{-# LANGUAGE AllowAmbiguousTypes #-}
{-# LANGUAGE ScopedTypeVariables #-}
{-# LANGUAGE TypeApplications #-}

module Main (main) where

import Control.Exception (SomeException, displayException, try)
import Control.Monad (unless, void, when)
import Data.Time (addDays, getCurrentTime, utctDay)
import Database.MySQL.Simple (Connection, close)
import Db (openConnection)
import Editable
import Entity
import Input
import Pretty
import Search
import System.IO (BufferMode (..), hSetBuffering, hSetEncoding, stdin, stdout, utf8)
import Text.Printf (printf)
import Types

main :: IO ()
main = do
  mapM_ (`hSetEncoding` utf8) [stdin, stdout]
  hSetBuffering stdout NoBuffering
  conn <- openConnection
  mainMenu conn
  close conn

menu :: String -> [(String, String)] -> IO String
menu header items = do
  putStrLn ""
  putStrLn ("=== " ++ header ++ " ===")
  mapM_ (\(k, v) -> putStrLn (k ++ ". " ++ v)) items
  ask "Оберіть пункт"

safely :: IO () -> IO ()
safely action = do
  result <- try action
  case result of
    Left (e :: SomeException) -> putStrLn ("Помилка: " ++ displayException e)
    Right () -> pure ()

mainMenu :: Connection -> IO ()
mainMenu conn = do
  choice <- menu "Програмне забезпечення на мережі факультету"
    [ ("1", "Каталог програмних засобів")
    , ("2", "Пошук")
    , ("3", "Додати програмний засіб")
    , ("4", "Редагувати програмний засіб")
    , ("5", "Видалити програмний засіб")
    , ("6", "Довідники: види, автори, користувачі")
    , ("7", "Зареєструвати використання")
    , ("8", "Додати відгук")
    , ("9", "Статистика використання та популярність")
    , ("0", "Вихід")
    ]
  let next action = safely action >> mainMenu conn
  case choice of
    "1" -> next (printAll =<< searchByText conn "")
    "2" -> next (searchMenu conn)
    "3" -> next (addSoftware conn)
    "4" -> next (editSoftware conn)
    "5" -> next (removeById @Software conn)
    "6" -> next (directories conn)
    "7" -> next (addRecord @Usage conn)
    "8" -> next (addRecord @Review conn)
    "9" -> next (statistics conn)
    "0" -> putStrLn "До побачення!"
    _   -> putStrLn "Невідомий пункт." >> mainMenu conn

searchMenu :: Connection -> IO ()
searchMenu conn = do
  choice <- menu "Пошук"
    [ ("1", "За назвою або текстом анотації")
    , ("2", "За автором")
    , ("3", "За видом")
    , ("4", "Ліцензії, що закінчуються протягом N днів")
    , ("0", "Назад")
    ]
  case choice of
    "1" -> printAll =<< searchByText conn =<< ask "Фрагмент назви або анотації"
    "2" -> printAll =<< searchByAuthor conn =<< ask "Фрагмент імені автора"
    "3" -> printAll =<< searchByCategory conn =<< ask "Фрагмент назви виду"
    "4" -> do
      days <- askInt "Кількість днів"
      today <- utctDay <$> getCurrentTime
      printAll =<< expiringBefore conn (addDays (fromIntegral days) today)
    _ -> pure ()

addSoftware :: Connection -> IO ()
addSoftware conn = do
  sw <- create @Software conn
  sid <- insertEntity conn sw
  putStrLn "Автори:"
  printAll =<< (fetchAll conn :: IO [Author])
  ids <- askIds "Коди авторів" []
  setAuthors conn sid ids
  putStrLn ("Додано програмний засіб з кодом " ++ show sid)

editSoftware :: Connection -> IO ()
editSoftware conn = do
  sid <- askInt "Код програмного засобу"
  found <- fetchById @Software conn sid
  case found of
    Nothing -> putStrLn "Програмний засіб не знайдено."
    Just sw -> do
      updated <- edit conn sw
      void (updateEntity conn updated)
      putStrLn "Автори:"
      printAll =<< (fetchAll conn :: IO [Author])
      current <- map authorId <$> authorsOf conn sid
      ids <- askIds "Коди авторів" current
      setAuthors conn sid ids
      putStrLn "Зміни збережено."

addRecord :: forall a. (Entity a, Editable a) => Connection -> IO ()
addRecord conn = do
  x <- create @a conn
  key <- insertEntity conn x
  putStrLn (title x ++ ": додано запис з кодом " ++ show key)

removeById :: forall a. Entity a => Connection -> IO ()
removeById conn = do
  key <- askInt "Код запису"
  confirm <- askDefault "Видалити? (так/ні)" "ні"
  when (confirm == "так") $ do
    n <- deleteEntity @a conn key
    putStrLn (if n > 0 then "Видалено." else "Запис не знайдено.")

directories :: Connection -> IO ()
directories conn = do
  choice <- menu "Довідники"
    [ ("1", "Види програмних засобів")
    , ("2", "Автори")
    , ("3", "Користувачі")
    , ("0", "Назад")
    ]
  case choice of
    "1" -> manage @Category conn "Види програмних засобів"
    "2" -> manage @Author conn "Автори"
    "3" -> manage @User conn "Користувачі"
    _   -> pure ()

manage :: forall a. (Entity a, Editable a, Pretty a) => Connection -> String -> IO ()
manage conn header = do
  choice <- menu header
    [ ("1", "Список")
    , ("2", "Додати")
    , ("3", "Редагувати")
    , ("4", "Видалити")
    , ("0", "Назад")
    ]
  let again action = safely action >> manage @a conn header
  case choice of
    "1" -> again (printAll =<< (fetchAll conn :: IO [a]))
    "2" -> again (addRecord @a conn)
    "3" -> again $ do
      key <- askInt "Код запису"
      found <- fetchById conn key
      case found of
        Nothing -> putStrLn "Запис не знайдено."
        Just (x :: a) -> do
          updated <- edit conn x
          void (updateEntity conn updated)
          putStrLn "Зміни збережено."
    "4" -> again (removeById @a conn)
    _   -> pure ()

statistics :: Connection -> IO ()
statistics conn = do
  putStrLn ""
  putStrLn "Популярність програмних засобів:"
  printf "%-26s %9s %12s %8s %8s\n" "Назва" "Запусків" "Користувачів" "Хвилин" "Оцінка"
  rows <- popularity conn
  mapM_ (\(name, runs, users, minutes, rating) ->
           printf "%-26s %9d %12d %8d %8s\n" name runs users minutes (maybe "—" (printf "%.2f") rating :: String))
        rows
  putStrLn ""
  putStrLn "Найактивніші користувачі:"
  top <- topUsers conn
  unless (null top) $
    mapM_ (\(name, unit, runs, minutes) ->
             printf "  %-24s %-30s %3d запусків, %4d хв\n" name unit runs minutes) top
