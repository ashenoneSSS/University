module Db
  ( openConnection
  ) where

import Data.Maybe (fromMaybe)
import Database.MySQL.Base.Types (Option (CharsetName))
import Database.MySQL.Simple
import System.Environment (lookupEnv)
import Text.Read (readMaybe)

openConnection :: IO Connection
openConnection = do
  host <- env "DB_HOST" "localhost"
  port <- env "DB_PORT" "3306"
  user <- env "DB_USER" "root"
  pass <- env "DB_PASSWORD" ""
  name <- env "DB_NAME" "faculty_software"
  connect defaultConnectInfo
    { connectHost = host
    , connectPort = fromMaybe 3306 (readMaybe port)
    , connectUser = user
    , connectPassword = pass
    , connectDatabase = name
    , connectOptions = [CharsetName "utf8mb4"]
    }
  where
    env key def = fromMaybe def <$> lookupEnv key
