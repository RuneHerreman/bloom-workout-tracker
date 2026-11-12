/**
 * Exercise search that understands gym shorthand ("rdl", "ohp", "db row") and
 * ranks what you meant above things that merely contain the letters.
 */

/** Shorthand → the words it stands for. Matched per typed word. */
const ALIASES: Record<string, string> = {
    rdl: "romanian deadlift",
    sldl: "stiff leg deadlift",
    sl: "single leg",
    dl: "deadlift",
    ohp: "overhead press",
    bp: "bench press",
    bss: "bulgarian split squat",
    ghr: "glute ham raise",
    db: "dumbbell",
    bb: "barbell",
    kb: "kettlebell",
    ez: "ez bar",
    tbar: "t bar",
    tri: "tricep",
    tris: "tricep",
    bi: "bicep",
    bis: "bicep",
    pushup: "push up",
    pushups: "push up",
    pullup: "pull up",
    pullups: "pull up",
    chinup: "chin up",
    chinups: "chin up",
    situp: "sit up",
    situps: "sit up",
    skullcrusher: "skull crusher",
    skullcrushers: "skull crusher",
};

const normalize = (text: string) =>
    text.toLowerCase().replace(/[-()/]/g, " ").replace(/\s+/g, " ").trim();

const expand = (query: string) =>
    normalize(query).split(" ").filter(Boolean).flatMap(word => (ALIASES[word] ?? word).split(" "));

/**
 * Lower is better; null means no match. 0: the name starts with what you typed,
 * 1–1.99: every (expanded) word starts a word of the name (fewer extra words first),
 * 2: the raw text appears somewhere in the name.
 */
export function matchScore(name: string, query: string): number | null {
    const q = normalize(query);
    if (!q) return 0;
    const n = normalize(name);
    if (n.startsWith(q)) return 0;
    const words = n.split(" ");
    const wanted = expand(query);
    if (wanted.length && wanted.every(w => words.some(word => word.startsWith(w)))) {
        // Fewer extra words is closer to what was meant: "rdl" → Romanian Deadlift before its variants.
        return 1 + Math.min(words.length - wanted.length, 99) / 100;
    }
    if (n.replace(/ /g, "").includes(q.replace(/ /g, ""))) return 2;
    return null;
}
