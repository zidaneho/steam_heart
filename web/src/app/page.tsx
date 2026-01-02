'use client'
import Image from "next/image";
import { useEffect, useState } from "react";
import axios from "axios";

interface Game {
  id: number;
  name: string;
  appId: number;
}

export default function Home() {
  const [games, setGames] = useState<Game[]>([]);
  useEffect(() => {
    axios
      .get("http://localhost:5116/api/games")
      .then((response) => {
        // Axios stores the actual JSON in the .data property
        setGames(response.data);
      })
      .catch((error) => {
        console.error("There was an error fetching the games!", error);
      });
  }, []);
  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-50 font-sans dark:bg-black">
      <main className="flex min-h-screen w-full max-w-3xl flex-col items-center justify-between py-32 px-16 bg-white dark:bg-black sm:items-start">
        <p>Zidane Ho</p>
        <ul>
          {games.map((game) => (
            <li key={game.id}>
              {game.name} (AppId: {game.appId})
            </li>
          ))}
        </ul>
      </main>
    </div>
  );
}
