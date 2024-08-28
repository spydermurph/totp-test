import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

const Twofactor = () => {
  const [token, setToken] = useState('');
  const navigate = useNavigate();
  const location = useLocation();
  const email = location.state.email;
  const provider = location.state.provider;

  const handleSubmit = async (e) => {
    e.preventDefault();
    const blog = { token, email, provider };
    try{
      const response = await fetch('https://localhost:44363/api/accounts/twofa', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(blog)
      });
      if(response.status === 200) {
        let data = await response.json();
        if(data.token !== null) {
          localStorage.setItem("token", data.token);
          navigate('/');
        } else {
          console.log("JWT missing");
        }
      } else if(response.status === 401) {
        console.log("Incorrect code");
        alert("Incorrect code")
      } else {
        console.log("Unknown error");
        alert("Unknown error");
      }
    } catch (err) {
      console.log(err);
      alert("Could not connect to server");
    }
    /*.then((response) => {
      return response.json();
    }).then((data) => {
      if(data.token !== null) {
        localStorage.setItem("token", data.token);
        navigate('/');
      }
    })*/
  }

  return (
    <div className="twofactor">
      <h2>Twofactor</h2>
      <p>{email}</p>
      <p>{provider}</p>
      <form onSubmit={handleSubmit}>
        <label>Token:</label>
        <input 
          type="text" 
          required 
          value={token}
          onChange={(e) => setToken(e.target.value)}
        />
        <button>Submit</button>
    </form>
    </div>
  );
}
 
export default Twofactor;