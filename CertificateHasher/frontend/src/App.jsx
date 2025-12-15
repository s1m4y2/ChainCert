import { useState } from "react";
import axios from "axios";

export default function App() {
    const [tab, setTab] = useState("verify");

    return (
        <div style={{ maxWidth: 720, margin: "40px auto", fontFamily: "Inter, system-ui" }}>
            <h1>MCBU Certificate dApp</h1>
            <div style={{ display: "flex", gap: 12, margin: "16px 0" }}>
                <button onClick={() => setTab("admin")}>Admin</button>
                <button onClick={() => setTab("verify")}>Verify</button>
            </div>
            {tab === "admin" ? <Admin /> : <Verify />}
        </div>
    );
}

function Admin() {
    const [file, setFile] = useState(null);
    const [to, setTo] = useState("");
    const [student, setStudent] = useState("");
    const [res, setRes] = useState(null);

    const issue = async () => {
        if (!file || !to || !student) return alert("Eksik bilgi!");
        const form = new FormData();
        form.append("file", file);
        form.append("to", to);
        form.append("student", student);
        const r = await axios.post("http://localhost:5000/api/issue", form, {
            headers: { "Content-Type": "multipart/form-data" }
        });
        setRes(r.data);
    };

    const revoke = async () => {
        const id = prompt("Token ID?");
        const reason = prompt("Reason?");
        const r = await axios.post("http://localhost:5000/api/revoke", null, { params: { tokenId: id, reason } });
        alert("Revoked tx: " + r.data.tx);
    };

    return (
        <div>
            <h2>Admin Panel</h2>
            <input type="file" onChange={e => setFile(e.target.files[0])} />
            <input placeholder="To Address" value={to} onChange={e => setTo(e.target.value)} />
            <input placeholder="Student Name" value={student} onChange={e => setStudent(e.target.value)} />
            <button onClick={issue}>Issue</button>
            <button onClick={revoke}>Revoke</button>
            {res && (
                <div style={{ marginTop: 16 }}>
                    <p>Tx: {res.tx}</p>
                    <p>SHA: {res.sha256}</p>
                    <p><a href={`https://ipfs.io/ipfs/${res.pdfCid}`} target="_blank">IPFS link</a></p>
                </div>
            )}
        </div>
    );
}

function Verify() {
    const [file, setFile] = useState(null);
    const [hash, setHash] = useState("");
    const [res, setRes] = useState(null);

    async function sha256(file) {
        const buf = await file.arrayBuffer();
        const digest = await crypto.subtle.digest("SHA-256", buf);
        return [...new Uint8Array(digest)].map(b => b.toString(16).padStart(2, "0")).join("");
    }

    const verify = async () => {
        let h = hash;
        if (file) h = await sha256(file);
        const r = await axios.get("http://localhost:5000/api/verify", { params: { hash: h } });
        setRes(r.data);
    };

    return (
        <div>
            <h2>Verify Certificate</h2>
            <input type="file" onChange={e => setFile(e.target.files[0])} />
            <input placeholder="or paste hash" value={hash} onChange={e => setHash(e.target.value)} />
            <button onClick={verify}>Check</button>
            {res && (
                <div style={{ marginTop: 16 }}>
                    {res.exists
                        ? (res.revoked ? <b>⛔ Revoked</b> : <b>✅ Valid</b>)
                        : <b>⚠️ Not Found</b>}
                    {res.tokenId ? <div>TokenId: {res.tokenId}</div> : null}
                </div>
            )}
        </div>
    );
}
