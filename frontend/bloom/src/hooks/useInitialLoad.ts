import { useState, useEffect } from "react";

export function useInitialLoad<T>(fetcher: () => Promise<T>): {
    data: T | null;
    loading: boolean;
    error: string | null;
} {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetcher()
            .then(setData)
            .catch(e => setError(e instanceof Error ? e.message : "Something went wrong"))
            .finally(() => setLoading(false));
    }, []);

    return { data, loading, error };
}
