{-# LANGUAGE AllowAmbiguousTypes #-}
{-# LANGUAGE ScopedTypeVariables #-}
{-# LANGUAGE TypeApplications #-}

module Entity
  ( Entity (..)
  , fetchAll
  , fetchById
  , insertEntity
  , updateEntity
  , deleteEntity
  ) where

import Data.Int (Int64)
import Data.List (intercalate)
import Data.String (fromString)
import Database.MySQL.Simple
import Database.MySQL.Simple.QueryParams (QueryParams)
import Database.MySQL.Simple.QueryResults (QueryResults)
import Types

class (QueryResults a, QueryParams a) => Entity a where
  tableName :: String
  columns   :: [String]
  entityKey :: a -> Int

selectSql :: forall a. Entity a => String
selectSql = "SELECT id, " ++ intercalate ", " (columns @a) ++ " FROM " ++ tableName @a

fetchAll :: forall a. Entity a => Connection -> IO [a]
fetchAll conn = query_ conn (fromString (selectSql @a ++ " ORDER BY id"))

fetchById :: forall a. Entity a => Connection -> Int -> IO (Maybe a)
fetchById conn key = do
  rows <- query conn (fromString (selectSql @a ++ " WHERE id = ?")) (Only key)
  pure $ case rows of
    (x : _) -> Just x
    []      -> Nothing

insertEntity :: forall a. Entity a => Connection -> a -> IO Int
insertEntity conn x = do
  let cols = columns @a
      sql = "INSERT INTO " ++ tableName @a ++ " (" ++ intercalate ", " cols ++ ") VALUES ("
            ++ intercalate ", " (map (const "?") cols) ++ ")"
  _ <- execute conn (fromString sql) x
  fromIntegral <$> insertID conn

updateEntity :: forall a. Entity a => Connection -> a -> IO Int64
updateEntity conn x = do
  let sets = intercalate ", " [c ++ " = ?" | c <- columns @a]
      sql = "UPDATE " ++ tableName @a ++ " SET " ++ sets ++ " WHERE id = " ++ show (entityKey x)
  execute conn (fromString sql) x

deleteEntity :: forall a. Entity a => Connection -> Int -> IO Int64
deleteEntity conn key =
  execute conn (fromString ("DELETE FROM " ++ tableName @a ++ " WHERE id = ?")) (Only key)

instance Entity Category where
  tableName = "categories"
  columns   = ["name"]
  entityKey = categoryId

instance Entity Author where
  tableName = "authors"
  columns   = ["full_name", "organization", "email"]
  entityKey = authorId

instance Entity Software where
  tableName = "software"
  columns   = [ "name", "annotation", "category_id", "version", "distribution_path"
              , "opened_at", "valid_until", "usage_conditions" ]
  entityKey = softwareId

instance Entity User where
  tableName = "users"
  columns   = ["full_name", "role", "unit", "email"]
  entityKey = userId

instance Entity Usage where
  tableName = "usage_log"
  columns   = ["software_id", "user_id", "used_on", "minutes"]
  entityKey = usageId

instance Entity Review where
  tableName = "reviews"
  columns   = ["software_id", "user_id", "rating", "comment", "created_on"]
  entityKey = reviewId
