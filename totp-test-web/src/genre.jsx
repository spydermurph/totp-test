import { useState } from "react";
import { useNavigate } from "react-router-dom";

const Genre = () => {
  const [genres, setGenres] = useState([]);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setGenres([])
    const response = await fetch('https://localhost:44363/api/genres', {
      method: 'GET',
      headers: { "Content-Type": "application/json", "Authorization": "Bearer " + localStorage.getItem("token") },
    }).then((response) => {
        return response.json();
    }).then((data) => {
        setGenres(data);
        //console.log(data);
    })
  }

  const handleClearTokens = async (e) => {
    e.preventDefault();
    const response = fetch('https://localhost:44363/api/accounts/clearalltests', {
      method: 'DELETE'
    })
  }

  return (
    <div className="genre">
      <h2>Genre</h2>
      <form onSubmit={handleClearTokens}>
        <button>Clear tests</button>
      </form>
      <p><button onClick={() => localStorage.removeItem("token")}>Logout</button></p>
      <p></p>
      <button onClick={() => navigate("/login")}>Login Identity OTP</button>
      <button onClick={() => navigate("/loginidentotp")}>Login Identity TOTP</button>
      <button onClick={() => navigate("/loginotp")}>Login OTP</button>
      <button onClick={() => navigate("/logintotp")}>Login TOTP</button>
      <form onSubmit={handleSubmit}>
        <button>Get Genre</button>
      </form>
      {genres.map((genre) => (
        <div className="genres" key={genre.id}>
          <p>{genre.name}</p>
        </div>
      ))}
    </div>
  );
}
 
export default Genre;