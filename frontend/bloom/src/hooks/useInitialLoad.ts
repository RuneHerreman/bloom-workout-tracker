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
        const ac = new AbortController();

        fetcher()
            .then(data => { if (!ac.signal.aborted) setData(data); })
            .catch(e => { if (!ac.signal.aborted) setError(e instanceof Error ? e.message : "Something went wrong"); })
            .finally(() => { if (!ac.signal.aborted) setLoading(false); });

        return () => ac.abort();
    }, []);

    return { data, loading, error };
}
