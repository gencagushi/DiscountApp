import React, { useEffect, useRef, useState } from "react";

function App() {
  const [log, setLog] = useState([]);
  const [ws, setWs] = useState(null);
  const [codes, setCodes] = useState([]);

  const countRef = useRef();
  const lengthRef = useRef();
  const codeRef = useRef();
  const logRef = useRef();

  const appendLog = (message) => {
    setLog((prev) => {
      const updated = [...prev, `${new Date().toLocaleTimeString()} — ${message}`];
      setTimeout(() => {
        if (logRef.current) {
          logRef.current.scrollTop = logRef.current.scrollHeight;
        }
      }, 0);
      return updated;
    });
  };

  useEffect(() => {
    const socket = new WebSocket("ws://localhost:5000/ws/");

    socket.onopen = () => {
      appendLog("✅ Connected to WebSocket");
      socket.send(JSON.stringify({ GetAll: true }));
    };

    socket.onmessage = (msg) => {
      try {
        const data = JSON.parse(msg.data);

        // 🔄 Full list of all codes (GetAll)
        if (Array.isArray(data.AllCodes)) {
          setCodes(data.AllCodes);
          appendLog(`🔄 Refreshed code list (${data.AllCodes.length} codes)`);
          return;
        }

        // ✅ Response from Generate
        if (Array.isArray(data.Codes) && typeof data.Codes[0] === "string") {
          appendLog(`✅ Generated codes: ${data.Codes.join(", ")}`);
          socket.send(JSON.stringify({ GetAll: true }));
          return;
        }

        // 🎉 Code used
        if (data.Result === 1) {
          appendLog("🎉 Code used successfully!");
          socket.send(JSON.stringify({ GetAll: true }));
          return;
        }

        // ❌ Code error
        if (data.Result === 0) {
          appendLog("❌ Invalid or already used code.");
          return;
        }

        appendLog(`🪵 Unknown response: ${msg.data}`);
      } catch (e) {
        appendLog(`⚠️ Error parsing response: ${msg.data}`);
      }
    };

    socket.onerror = () => appendLog("WebSocket error");
    socket.onclose = () => appendLog("WebSocket closed");
    setWs(socket);

    return () => socket.close();
  }, []);

  const sendGenerate = () => {
    const count = parseInt(countRef.current.value, 10);
    const length = parseInt(lengthRef.current.value, 10);
    const payload = JSON.stringify({ Count: count, Length: length });
    ws.send(payload);
    appendLog(`Client: Sent Generate request -> ${payload}`);
  };

  const sendUseCode = () => {
    const code = codeRef.current.value;
    const payload = JSON.stringify({ Code: code });
    ws.send(payload);
    appendLog(`Client: Sent UseCode request -> ${payload}`);
  };

  return (
    <div style={{ padding: 20, fontFamily: "sans-serif" }}>
      <h2>🎟️ Discount Code Client (WebSocket)</h2>

      <div style={{ marginBottom: 20 }}>
        <h4>Generate Codes</h4>
        <input ref={countRef} placeholder="Count" type="number" defaultValue={5} />{" "}
        <input ref={lengthRef} placeholder="Length" type="number" defaultValue={8} />{" "}
        <button onClick={sendGenerate}>Generate</button>
      </div>

      <div style={{ marginBottom: 20 }}>
        <h4>Use a Discount Code</h4>
        <input ref={codeRef} placeholder="Enter Code" />{" "}
        <button onClick={sendUseCode}>Use Code</button>
      </div>

      <div>
        <h4>Log:</h4>
        <div
          ref={logRef}
          style={{
            whiteSpace: "pre-wrap",
            background: "#f0f0f0",
            padding: 10,
            maxHeight: 300,
            overflowY: "auto",
            border: "1px solid #ccc",
            borderRadius: "5px",
          }}
        >
          {log.map((entry, i) => (
            <div key={i}>{entry}</div>
          ))}
        </div>
      </div>

      <div style={{ marginTop: "2rem" }}>
        <h4>All Discount Codes</h4>
        <table
          style={{
            width: "100%",
            borderCollapse: "collapse",
            marginTop: "1rem",
            border: "1px solid #ccc",
          }}
        >
          <thead>
            <tr>
              <th style={thStyle}>Code</th>
              <th style={thStyle}>Used</th>
            </tr>
          </thead>
          <tbody>
            {codes.length > 0 ? (
              codes.map((entry, index) => (
                <tr key={index}>
                  <td style={tdStyle}>{entry.Code ?? entry.code}</td>
                  <td style={tdStyle}>{entry.Used ?? entry.used ? "✅ Used" : "❌ Not used"}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td style={tdStyle} colSpan={2}>
                  No codes yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

const thStyle = {
  border: "1px solid #ccc",
  padding: "8px",
  backgroundColor: "#eee",
  textAlign: "left",
};

const tdStyle = {
  border: "1px solid #ccc",
  padding: "8px",
};

export default App;
