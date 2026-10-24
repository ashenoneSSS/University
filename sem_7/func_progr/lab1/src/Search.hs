{-# LANGUAGE OverloadedStrings #-}

module Search
  ( SoftwareRow (..)
  , searchByText
  , searchByAuthor
  , searchByCategory
  , authorsOf
  , setAuthors
  , popularity
  , topUsers
  , expiringBefore
  ) where

import Control.Monad (forM_, void)
import Data.List (intercalate)
import Data.Time (Day)
import Database.MySQL.Simple
import Entity (fetchAll)
import Pretty (Pretty (..))
import Types

data SoftwareRow = SoftwareRow
  { rowSoftware :: Software
  , rowCategory :: String
  , rowAuthors  :: [String]
  }

like :: String -> String
like s = "%" ++ s ++ "%"

withDetails :: Connection -> [Software] -> IO [SoftwareRow]
withDetails conn = mapM $ \s -> do
  [Only cat] <- query conn "SELECT name FROM categories WHERE id = ?" (Only (softwareCategoryId s))
  names <- map authorName <$> authorsOf conn (softwareId s)
  pure (SoftwareRow s cat names)

softwareSelect :: Query
softwareSelect =
  "SELECT DISTINCT s.id, s.name, s.annotation, s.category_id, s.version, s.distribution_path, \
  \s.opened_at, s.valid_until, s.usage_conditions FROM software s "

searchByText :: Connection -> String -> IO [SoftwareRow]
searchByText conn text = do
  rows <- query conn (softwareSelect <> "WHERE s.name LIKE ? OR s.annotation LIKE ? ORDER BY s.name")
                     (like text, like text)
  withDetails conn rows

searchByAuthor :: Connection -> String -> IO [SoftwareRow]
searchByAuthor conn text = do
  rows <- query conn (softwareSelect <> "JOIN software_authors sa ON sa.software_id = s.id \
                       \JOIN authors a ON a.id = sa.author_id WHERE a.full_name LIKE ? ORDER BY s.name")
                     (Only (like text))
  withDetails conn rows

searchByCategory :: Connection -> String -> IO [SoftwareRow]
searchByCategory conn text = do
  rows <- query conn (softwareSelect <> "JOIN categories c ON c.id = s.category_id \
                       \WHERE c.name LIKE ? ORDER BY s.name")
                     (Only (like text))
  withDetails conn rows

expiringBefore :: Connection -> Day -> IO [SoftwareRow]
expiringBefore conn day = do
  rows <- query conn (softwareSelect <> "WHERE s.valid_until IS NOT NULL AND s.valid_until <= ? \
                       \ORDER BY s.valid_until")
                     (Only day)
  withDetails conn rows

authorsOf :: Connection -> Int -> IO [Author]
authorsOf conn sid =
  query conn "SELECT a.id, a.full_name, a.organization, a.email FROM authors a \
             \JOIN software_authors sa ON sa.author_id = a.id WHERE sa.software_id = ? ORDER BY a.full_name"
        (Only sid)

setAuthors :: Connection -> Int -> [Int] -> IO ()
setAuthors conn sid ids = do
  void $ execute conn "DELETE FROM software_authors WHERE software_id = ?" (Only sid)
  known <- map authorId <$> (fetchAll conn :: IO [Author])
  forM_ (filter (`elem` known) ids) $ \aid ->
    execute conn "INSERT INTO software_authors (software_id, author_id) VALUES (?, ?)" (sid, aid)

popularity :: Connection -> IO [(String, Int, Int, Int, Maybe Double)]
popularity conn =
  query_ conn
    "SELECT s.name, \
    \  (SELECT COUNT(*) FROM usage_log u WHERE u.software_id = s.id), \
    \  (SELECT COUNT(DISTINCT u.user_id) FROM usage_log u WHERE u.software_id = s.id), \
    \  CAST((SELECT COALESCE(SUM(u.minutes), 0) FROM usage_log u WHERE u.software_id = s.id) AS SIGNED), \
    \  (SELECT AVG(r.rating) FROM reviews r WHERE r.software_id = s.id) \
    \FROM software s ORDER BY 2 DESC, 4 DESC"

topUsers :: Connection -> IO [(String, String, Int, Int)]
topUsers conn =
  query_ conn
    "SELECT us.full_name, us.unit, COUNT(*), CAST(SUM(u.minutes) AS SIGNED) FROM usage_log u \
    \JOIN users us ON us.id = u.user_id GROUP BY us.id, us.full_name, us.unit ORDER BY 4 DESC"

instance Pretty SoftwareRow where
  pretty (SoftwareRow s cat names) =
    pretty s ++ "    Вид:        " ++ cat ++ "\n    Автори:     "
      ++ (if null names then "—" else intercalate ", " names)
